// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.Logging;
using Microsoft.AspNet.Mvc.DecisionTree;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.Routing
{
    /// <summary>
    /// An <see cref="IRouter"/> implementation for attribute routing.
    /// </summary>
    public class AttributeRoute : IRouter
    {
        private readonly IRouter _next;
        private readonly TemplateRoute[] _matchingRoutes;
        private readonly AttributeRouteLinkGenerationEntry[] _linkGenerationEntries;
        private ILogger _logger;
        private ILogger _constraintLogger;
        private readonly LinkGenerationDecisionTree _tree;

        /// <summary>
        /// Creates a new <see cref="AttributeRoute"/>.
        /// </summary>
        /// <param name="next">The next router. Invoked when a route entry matches.</param>
        /// <param name="entries">The set of route entries.</param>
        public AttributeRoute(
            [NotNull] IRouter next,
            [NotNull] IEnumerable<AttributeRouteMatchingEntry> matchingEntries,
            [NotNull] IEnumerable<AttributeRouteLinkGenerationEntry> linkGenerationEntries,
            [NotNull] ILoggerFactory factory)
        {
            _next = next;

            // Order all the entries by order, then precedence, and then finally by template in order to provide
            // a stable routing and link generation order for templates with same order and precedence. 
            // We use ordinal comparison for the templates because we only care about them being exactly equal and
            // we don't want to make any equivalence between templates based on the culture of the machine.

            _matchingRoutes = matchingEntries
                .OrderBy(o => o.Order)
                .ThenBy(e => e.Precedence)
                .ThenBy(e => e.Route.RouteTemplate, StringComparer.Ordinal)
                .Select(e => e.Route)
                .ToArray();

            _linkGenerationEntries = linkGenerationEntries
                .OrderBy(o => o.Order)
                .ThenBy(e => e.Precedence)
                .ThenBy(e => e.TemplateText, StringComparer.Ordinal)
                .ToArray();

            _logger = factory.Create<AttributeRoute>();
            _constraintLogger = factory.Create(typeof(RouteConstraintMatcher).FullName);
            _tree = new LinkGenerationDecisionTree(linkGenerationEntries);
        }

        /// <inheritdoc />
        public async Task RouteAsync([NotNull] RouteContext context)
        {
            using (_logger.BeginScope("AttributeRoute.RouteAsync"))
            {
            foreach (var route in _matchingRoutes)
            {
                await route.RouteAsync(context);

                if (context.IsHandled)
                {
                        break;
                    }
                }

                if (_logger.IsEnabled(TraceType.Information))
                {
                    _logger.WriteValues(new AttributeRouteRouteAsyncValues()
                    {
                        MatchingRoutes = _matchingRoutes,
                        Handled = context.IsHandled
                    });
                }
            }
        }

        /// <inheritdoc />
        public string GetVirtualPath([NotNull] VirtualPathContext context)
        {
            // To generate a link, we iterate the collection of entries (in order of precedence) and execute
            // each one that matches the 'required link values' - which will typically be a value for action
            // and controller.
            //
            // Building a proper data structure to optimize this is tracked by #741
            var matches = _tree.Select(context);
            foreach (var entry in matches)
            {
                var path = GenerateLink(context, entry);
                if (path != null)
                {
                    context.IsBound = true;
                    return path;
                }
            }

            return null;
        }

        private string GenerateLink(VirtualPathContext context, AttributeRouteLinkGenerationEntry entry)
        {
            // In attribute the context includes the values that are used to select this entry - typically
            // these will be the standard 'action', 'controller' and maybe 'area' tokens. However, we don't
            // want to pass these to the link generation code, or else they will end up as query parameters.
            //
            // So, we need to exclude from here any values that are 'required link values', but aren't
            // parameters in the template.
            //
            // Ex: 
            //      template: api/Products/{action}
            //      required values: { id = "5", action = "Buy", Controller = "CoolProducts" }
            //
            //      result: { id = "5", action = "Buy" }
            var inputValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in context.Values)
            {
                if (entry.RequiredLinkValues.ContainsKey(kvp.Key))
                {
                    var parameter = entry.Template.Parameters
                        .FirstOrDefault(p => string.Equals(p.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));

                    if (parameter == null)
                    {
                        continue;
                    }
                }

                inputValues.Add(kvp.Key, kvp.Value);
            }

            var bindingResult = entry.Binder.GetValues(context.AmbientValues, inputValues);
            if (bindingResult == null)
            {
                // A required parameter in the template didn't get a value.
                return null;
            }

            var matched = RouteConstraintMatcher.Match(
                entry.Constraints,
                bindingResult.CombinedValues,
                context.Context,
                this,
                RouteDirection.UrlGeneration,
                _constraintLogger);

            if (!matched)
            {
                // A constraint rejected this link.
                return null;
            }

            // These values are used to signal to the next route what we would produce if we round-tripped
            // (generate a link and then parse). In MVC the 'next route' is typically the MvcRouteHandler.
            var providedValues = new Dictionary<string, object>(
                bindingResult.AcceptedValues,
                StringComparer.OrdinalIgnoreCase);
            providedValues.Add(AttributeRouting.RouteGroupKey, entry.RouteGroup);

            var childContext = new VirtualPathContext(context.Context, context.AmbientValues, context.Values)
            {
                ProvidedValues = providedValues,
            };

            var path = _next.GetVirtualPath(childContext);
            if (path != null)
            {
                // If path is non-null then the target router short-circuited, we don't expect this
                // in typical MVC scenarios.
                return path;
            }
            else if (!childContext.IsBound)
            {
                // The target router has rejected these values. We don't expect this in typical MVC scenarios.
                return null;
            }

            path = entry.Binder.BindValues(bindingResult.AcceptedValues);
            return path;
        }

        private bool ContextHasSameValue(VirtualPathContext context, string key, object value)
        {
            object providedValue;
            if (!context.Values.TryGetValue(key, out providedValue))
            {
                // If the required value is an 'empty' route value, then ignore ambient values.
                // This handles a case where we're generating a link to an action like:
                // { area = "", controller = "Home", action = "Index" } 
                //
                // and the ambient values has a value for area.
                if (value != null)
                {
                    context.AmbientValues.TryGetValue(key, out providedValue);
                }
            }

            return TemplateBinder.RoutePartsEqual(providedValue, value);
        }

        private class LinkGenerationDecisionTree
        {
            private readonly DecisionTreeNode<AttributeRouteLinkGenerationEntry, object> _root;

            public LinkGenerationDecisionTree(IReadOnlyList<AttributeRouteLinkGenerationEntry> entries)
            {
                var fullTree = DecisionTreeBuilder<AttributeRouteLinkGenerationEntry, object>.GenerateTree(entries, new AttributeRouteLinkGenerationEntryClassifier());
                _root = DecisionTreeBuilder<AttributeRouteLinkGenerationEntry, object>.Optimize(fullTree, new HashSet<AttributeRouteLinkGenerationEntry>());
            }

            public List<AttributeRouteLinkGenerationEntry> Select(VirtualPathContext context)
            {
                var results = new List<AttributeRouteLinkGenerationEntry>();
                Walk(results, context, _root);
                results.Sort(new AttributeRouteLinkGenerationEntryComparer());
                return results;
            }

            private void Walk(List<AttributeRouteLinkGenerationEntry> results, VirtualPathContext context, DecisionTreeNode<AttributeRouteLinkGenerationEntry, object> node)
            {
                for (int i = 0; i < node.Matches.Count; i++)
                {
                    results.Add(node.Matches[i].Item);
                }

                for (int i = 0; i < node.Criteria.Count; i++)
                {
                    var criterion = node.Criteria[i];
                    var key = criterion.Key;

                    object value;
                    if (context.Values.TryGetValue(key, out value))
                    {
                        DecisionTreeNode<AttributeRouteLinkGenerationEntry, object> branch;
                        if (criterion.Branches.TryGetValue(value ?? string.Empty, out branch))
                        {
                            Walk(results, context, branch);
                        }
                    }
                    else
                    {
                        // If a value wasn't explicitly supplied, match BOTH the ambient value and the empty value
                        // if an ambient value was supplied.
                        DecisionTreeNode<AttributeRouteLinkGenerationEntry, object> branch;
                        if (context.AmbientValues.TryGetValue(key, out value) &&
                            !criterion.Branches.Comparer.Equals(value, string.Empty))
                        {
                            if (criterion.Branches.TryGetValue(value, out branch))
                            {
                                Walk(results, context, branch);
                            }
                        }

                        if (criterion.Branches.TryGetValue(string.Empty, out branch))
                        {
                            Walk(results, context, branch);
                        }
                    }

                }
            }
        }

        private class AttributeRouteLinkGenerationEntryClassifier : IClassifier<AttributeRouteLinkGenerationEntry, object>
        {
            public AttributeRouteLinkGenerationEntryClassifier()
            {
                ValueComparer = new RouteValueEqualityComparer();
            }

            public IEqualityComparer<object> ValueComparer { get; private set; }

            public IDictionary<string, object> GetCriteria(AttributeRouteLinkGenerationEntry item)
            {
                return item.RequiredLinkValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ?? string.Empty, StringComparer.OrdinalIgnoreCase);
            }
        }

        private class RouteValueEqualityComparer : IEqualityComparer<object>
        {
            public new bool Equals(object x, object y)
            {
                var stringX = x as string ?? Convert.ToString(x, CultureInfo.InvariantCulture);
                var stringY = y as string ?? Convert.ToString(y, CultureInfo.InvariantCulture);

                if (string.IsNullOrEmpty(stringX) && string.IsNullOrEmpty(stringY))
                {
                    return true;
                }
                else
                {
                    return string.Equals(stringX, stringY, StringComparison.OrdinalIgnoreCase);
                }
            }

            public int GetHashCode(object obj)
            {
                var stringObj = obj as string ?? Convert.ToString(obj, CultureInfo.InvariantCulture);
                if (string.IsNullOrEmpty(stringObj))
                {
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(string.Empty);
                }
                else
                {
                    return StringComparer.OrdinalIgnoreCase.GetHashCode(stringObj);
                }
            }
        }

        private class AttributeRouteLinkGenerationEntryComparer : IComparer<AttributeRouteLinkGenerationEntry>
        {
            public int Compare(AttributeRouteLinkGenerationEntry x, AttributeRouteLinkGenerationEntry y)
            {
                return x.Precedence.CompareTo(y.Precedence);
            }
        }
    }
}
