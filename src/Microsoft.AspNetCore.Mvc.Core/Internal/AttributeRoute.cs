// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Core;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Internal;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class AttributeRoute : IRouter
    {
        private readonly IRouter _target;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
        private readonly IInlineConstraintResolver _constraintResolver;
        private readonly ObjectPool<UriBuildingContext> _contextPool;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILoggerFactory _loggerFactory;

        private TreeRouter _router;

        public AttributeRoute(
            IRouter target,
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IInlineConstraintResolver constraintResolver,
            ObjectPool<UriBuildingContext> contextPool,
            UrlEncoder urlEncoder,
            ILoggerFactory loggerFactory)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (actionDescriptorCollectionProvider == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptorCollectionProvider));
            }

            if (constraintResolver == null)
            {
                throw new ArgumentNullException(nameof(constraintResolver));
            }

            if (contextPool == null)
            {
                throw new ArgumentNullException(nameof(contextPool));
            }

            if (urlEncoder == null)
            {
                throw new ArgumentNullException(nameof(urlEncoder));
            }

            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            _target = target;
            _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            _constraintResolver = constraintResolver;
            _contextPool = contextPool;
            _urlEncoder = urlEncoder;
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            var router = GetTreeRouter();
            return router.GetVirtualPath(context);
        }

        /// <inheritdoc />
        public Task RouteAsync(RouteContext context)
        {
            var router = GetTreeRouter();
            return router.RouteAsync(context);
        }

        private TreeRouter GetTreeRouter()
        {
            var actions = _actionDescriptorCollectionProvider.ActionDescriptors;

            // This is a safe-race. We'll never set router back to null after initializing
            // it on startup.
            if (_router == null || _router.Version != actions.Version)
            {
                _router = BuildRoute(actions);
            }

            return _router;
        }

        private TreeRouter BuildRoute(ActionDescriptorCollection actions)
        {
            var routeBuilder = new TreeRouteBuilder(_target, _loggerFactory);
            var routeInfos = GetRouteInfos(_constraintResolver, actions.Items);

            // We're creating one AttributeRouteGenerationEntry per action. This allows us to match the intended
            // action by expected route values, and then use the TemplateBinder to generate the link.
            foreach (var routeInfo in routeInfos)
            {
                routeBuilder.Add(new TreeRouteLinkGenerationEntry()
                {
                    Binder = new TemplateBinder(_urlEncoder, _contextPool, routeInfo.ParsedTemplate, routeInfo.Defaults),
                    Defaults = routeInfo.Defaults,
                    Constraints = routeInfo.Constraints,
                    Order = routeInfo.Order,
                    GenerationPrecedence = routeInfo.GenerationPrecedence,
                    RequiredLinkValues = routeInfo.ActionDescriptor.RouteValueDefaults,
                    RouteGroup = routeInfo.RouteGroup,
                    Template = routeInfo.ParsedTemplate,
                    Name = routeInfo.Name,
                });
            }

            // We're creating one AttributeRouteMatchingEntry per group, so we need to identify the distinct set of
            // groups. It's guaranteed that all members of the group have the same template and precedence,
            // so we only need to hang on to a single instance of the RouteInfo for each group.
            var distinctRouteInfosByGroup = GroupRouteInfosByGroupId(routeInfos);
            foreach (var routeInfo in distinctRouteInfosByGroup)
            {
                routeBuilder.Add(new TreeRouteMatchingEntry()
                {
                    Order = routeInfo.Order,
                    Precedence = routeInfo.MatchPrecedence,
                    Target = _target,
                    RouteName = routeInfo.Name,
                    RouteTemplate = TemplateParser.Parse(routeInfo.RouteTemplate),
                    TemplateMatcher = new TemplateMatcher(
                        routeInfo.ParsedTemplate,
                        new RouteValueDictionary(StringComparer.OrdinalIgnoreCase)
                        {
                            { TreeRouter.RouteGroupKey, routeInfo.RouteGroup }
                        }),
                    Constraints = routeInfo.Constraints
                });
            }

            return routeBuilder.Build(actions.Version);
        }

        private static IEnumerable<RouteInfo> GroupRouteInfosByGroupId(List<RouteInfo> routeInfos)
        {
            var routeInfosByGroupId = new Dictionary<string, RouteInfo>(StringComparer.OrdinalIgnoreCase);

            foreach (var routeInfo in routeInfos)
            {
                if (!routeInfosByGroupId.ContainsKey(routeInfo.RouteGroup))
                {
                    routeInfosByGroupId.Add(routeInfo.RouteGroup, routeInfo);
                }
            }

            return routeInfosByGroupId.Values;
        }

        private static List<RouteInfo> GetRouteInfos(
            IInlineConstraintResolver constraintResolver,
            IReadOnlyList<ActionDescriptor> actions)
        {
            var routeInfos = new List<RouteInfo>();
            var errors = new List<RouteInfo>();

            // This keeps a cache of 'Template' objects. It's a fairly common case that multiple actions
            // will use the same route template string; thus, the `Template` object can be shared.
            //
            // For a relatively simple route template, the `Template` object will hold about 500 bytes
            // of memory, so sharing is worthwhile.
            var templateCache = new Dictionary<string, RouteTemplate>(StringComparer.OrdinalIgnoreCase);

            var attributeRoutedActions = actions.Where(a => a.AttributeRouteInfo != null &&
                a.AttributeRouteInfo.Template != null);
            foreach (var action in attributeRoutedActions)
            {
                var routeInfo = GetRouteInfo(constraintResolver, templateCache, action);
                if (routeInfo.ErrorMessage == null)
                {
                    routeInfos.Add(routeInfo);
                }
                else
                {
                    errors.Add(routeInfo);
                }
            }

            if (errors.Count > 0)
            {
                var allErrors = string.Join(
                    Environment.NewLine + Environment.NewLine,
                    errors.Select(
                        e => Resources.FormatAttributeRoute_IndividualErrorMessage(
                            e.ActionDescriptor.DisplayName,
                            Environment.NewLine,
                            e.ErrorMessage)));

                var message = Resources.FormatAttributeRoute_AggregateErrorMessage(Environment.NewLine, allErrors);
                throw new InvalidOperationException(message);
            }

            return routeInfos;
        }

        private static RouteInfo GetRouteInfo(
            IInlineConstraintResolver constraintResolver,
            Dictionary<string, RouteTemplate> templateCache,
            ActionDescriptor action)
        {
            var constraint = action.RouteConstraints
                .Where(c => c.RouteKey == TreeRouter.RouteGroupKey)
                .FirstOrDefault();
            if (constraint == null ||
                constraint.KeyHandling != RouteKeyHandling.RequireKey ||
                constraint.RouteValue == null)
            {
                // This can happen if an ActionDescriptor has a route template, but doesn't have one of our
                // special route group constraints. This is a good indication that the user is using a 3rd party
                // routing system, or has customized their ADs in a way that we can no longer understand them.
                //
                // We just treat this case as an 'opt-out' of our attribute routing system.
                return null;
            }

            var routeInfo = new RouteInfo()
            {
                ActionDescriptor = action,
                RouteGroup = constraint.RouteValue,
                RouteTemplate = action.AttributeRouteInfo.Template,
            };

            try
            {
                RouteTemplate parsedTemplate;
                if (!templateCache.TryGetValue(action.AttributeRouteInfo.Template, out parsedTemplate))
                {
                    // Parsing with throw if the template is invalid.
                    parsedTemplate = TemplateParser.Parse(action.AttributeRouteInfo.Template);
                    templateCache.Add(action.AttributeRouteInfo.Template, parsedTemplate);
                }

                routeInfo.ParsedTemplate = parsedTemplate;
            }
            catch (Exception ex)
            {
                routeInfo.ErrorMessage = ex.Message;
                return routeInfo;
            }

            foreach (var kvp in action.RouteValueDefaults)
            {
                foreach (var parameter in routeInfo.ParsedTemplate.Parameters)
                {
                    if (string.Equals(kvp.Key, parameter.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        routeInfo.ErrorMessage = Resources.FormatAttributeRoute_CannotContainParameter(
                            routeInfo.RouteTemplate,
                            kvp.Key,
                            kvp.Value);

                        return routeInfo;
                    }
                }
            }

            routeInfo.Order = action.AttributeRouteInfo.Order;

            routeInfo.MatchPrecedence = RoutePrecedence.ComputeMatched(routeInfo.ParsedTemplate);
            routeInfo.GenerationPrecedence = RoutePrecedence.ComputeGenerated(routeInfo.ParsedTemplate);

            routeInfo.Name = action.AttributeRouteInfo.Name;

            var constraintBuilder = new RouteConstraintBuilder(constraintResolver, routeInfo.RouteTemplate);

            foreach (var parameter in routeInfo.ParsedTemplate.Parameters)
            {
                if (parameter.InlineConstraints != null)
                {
                    if (parameter.IsOptional)
                    {
                        constraintBuilder.SetOptional(parameter.Name);
                    }

                    foreach (var inlineConstraint in parameter.InlineConstraints)
                    {
                        constraintBuilder.AddResolvedConstraint(parameter.Name, inlineConstraint.Constraint);
                    }
                }
            }

            routeInfo.Constraints = constraintBuilder.Build();

            routeInfo.Defaults = new RouteValueDictionary();
            foreach (var parameter in routeInfo.ParsedTemplate.Parameters)
            {
                if (parameter.DefaultValue != null)
                {
                    routeInfo.Defaults.Add(parameter.Name, parameter.DefaultValue);
                }
            }

            return routeInfo;
        }

        private class RouteInfo
        {
            public ActionDescriptor ActionDescriptor { get; set; }

            public IDictionary<string, IRouteConstraint> Constraints { get; set; }

            public RouteValueDictionary Defaults { get; set; }

            public string ErrorMessage { get; set; }

            public RouteTemplate ParsedTemplate { get; set; }

            public int Order { get; set; }

            public decimal MatchPrecedence { get; set; }

            public decimal GenerationPrecedence { get; set; }

            public string RouteGroup { get; set; }

            public string RouteTemplate { get; set; }

            public string Name { get; set; }
        }
    }
}
