// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.Routing
{
    internal class ControllerEndpointDataSource : EndpointDataSource, IEndpointConventionBuilder
    {
        private readonly Func<Endpoint[]> _initializer;

        private readonly List<ControllerActionEndpointModel> _models;
        private readonly List<ControllerActionEndpointModel> _unmappedModels;

        private object _lock;
        private bool _initialized;
        private Endpoint[] _endpoints;
        private CancellationChangeToken _changeToken;
        private CancellationTokenSource _cancellationTokenSource;

        // TODO: there is a bug here due to filtering. If some data sources see a conventional route and others
        // don't think this will be inconsistent.
        //
        // Order value used for conventionally routed actions. Incremented for each route added.
        private int _nextConventionalRouteOrder = 1;
        private StringBuilder _templateBuilder;

        public ControllerEndpointDataSource(
            Assembly assembly,
            List<ControllerActionEndpointModel> models,
            List<ControllerActionEndpointModel> unmappedModels)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            if (models == null)
            {
                throw new ArgumentNullException(nameof(models));
            }

            if (unmappedModels == null)
            {
                throw new ArgumentNullException(nameof(unmappedModels));
            }

            Assembly = assembly;
            _models = models;
            _unmappedModels = unmappedModels;

            _initializer = CreateEndpoints;

            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);
            _lock = new object();
        }

        public Assembly Assembly { get; }

        public override IReadOnlyList<Endpoint> Endpoints
        {
            get
            {
                return LazyInitializer.EnsureInitialized(ref _endpoints, ref _initialized, ref _lock, _initializer);
            }
        }

        public void Apply(Action<ControllerActionEndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            for (var i = 0; i < _models.Count; i++)
            {
                convention(_models[i]);
            }
        }

        void IEndpointConventionBuilder.Apply(Action<EndpointModel> convention)
        {
            if (convention == null)
            {
                throw new ArgumentNullException(nameof(convention));
            }

            Apply(convention);
        }

        public override IChangeToken GetChangeToken() => _changeToken;

        public void MapControllerRoute(string name, string template, object defaults, object constraints, object dataTokens)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            var pattern = RoutePatternFactory.Parse(template, defaults, constraints);

            lock (_lock)
            {
                var models = _unmappedModels;
                for (var i = 0; i < models.Count; i++)
                {
                    ProcessUnmappedModel(
                        models[i], 
                        name, 
                        pattern, 
                        new RouteValueDictionary(defaults), 
                        new RouteValueDictionary(constraints),
                        new Dictionary<string, IList<IParameterPolicy>>(StringComparer.OrdinalIgnoreCase), // TODO something 
                        new RouteValueDictionary(dataTokens));
                }
            }
        }

        private void Update(Endpoint[] endpoints)
        {
            // See comments in DefaultActionDescriptorCollectionProvider. These steps are done
            // in a specific order to ensure callers always see a consistent state.

            // Step 1 - capture old token
            var oldCancellationTokenSource = _cancellationTokenSource;

            // Step 2 - update endpoints
            _endpoints = endpoints;

            // Step 3 - create new change token
            _cancellationTokenSource = new CancellationTokenSource();
            _changeToken = new CancellationChangeToken(_cancellationTokenSource.Token);

            // Step 4 - trigger old token
            oldCancellationTokenSource?.Cancel();
        }

        // Must be called with the lock held.
        private Endpoint[] CreateEndpoints()
        {
            var models = _models;
            var endpoints = new Endpoint[models.Count];
            for (var i = 0; i < models.Count; i++)
            {
                var model = models[i];
                endpoints[i] = model.Build();
            }

            return endpoints;
        }

        private void ProcessUnmappedModel(
            ControllerActionEndpointModel model, 
            string name,
            RoutePattern pattern, 
            RouteValueDictionary defaults,
            RouteValueDictionary constraints,
            Dictionary<string, IList<IParameterPolicy>> parameterPolicies,
            RouteValueDictionary dataTokens)
        {
            // In traditional conventional routing setup, the routes defined by a user have a static order
            // defined by how they are added into the list. We would like to maintain the same order when building
            // up the endpoints too.
            //
            // Start with an order of '1' for conventional routes as attribute routes have a default order of '0'.
            // This is for scenarios dealing with migrating existing Router based code to Endpoint Routing world.
            var order = _nextConventionalRouteOrder;

            // Check each of the conventional patterns to see if the action would be reachable
            // If the action and pattern are compatible then create an endpoint with the
            // area/controller/action parameter parts replaced with literals
            //
            // e.g. {controller}/{action} with HomeController.Index and HomeController.Login
            // would result in endpoints:
            // - Home/Index
            // - Home/Login

            // An 'endpointInfo' is applicable if:
            // 1. it has a parameter (or default value) for 'required' non-null route value
            // 2. it does not have a parameter (or default value) for 'required' null route value

            var requiredValues = new Dictionary<string, string>(model.RequiredValues, StringComparer.OrdinalIgnoreCase);
            if (!requiredValues.ContainsKey("action"))
            {
                requiredValues.Add("action", model.ActionName);
            }
            if (!requiredValues.ContainsKey("controller"))
            {
                requiredValues.Add("controller", model.ControllerName);
            }

            foreach (var routeKey in requiredValues.Keys)
            {
                if (!MatchRouteValue(requiredValues, pattern, defaults, parameterPolicies, routeKey))
                {
                    return;
                }
            }

            _nextConventionalRouteOrder = CreateEndpoints(
                model,
                order,
                name,
                pattern,
                defaults,
                constraints,
                dataTokens,
                parameterPolicies,
                suppressLinkGeneration: false,
                suppressPathMatching: false);
        }

        // CreateEndpoints processes the route pattern, replacing area/controller/action parameters with endpoint values
        // Because of default values it is possible for a route pattern to resolve to multiple endpoints
        private int CreateEndpoints(
            ControllerActionEndpointModel model,
            int order,
            string name,
            RoutePattern pattern,
            IReadOnlyDictionary<string, object> nonInlineDefaults,
            RouteValueDictionary constraints,
            RouteValueDictionary dataTokens,
            IDictionary<string, IList<IParameterPolicy>> parameterPolicies,
            bool suppressLinkGeneration,
            bool suppressPathMatching)
        {
            var newPathSegments = pattern.PathSegments.ToList();
            var hasLinkGenerationEndpoint = false;

            // Create a mutable copy
            var nonInlineDefaultsCopy = nonInlineDefaults != null
                ? new RouteValueDictionary(nonInlineDefaults)
                : null;

            var resolvedRouteValues = ResolveActionRouteValues(model, pattern.Defaults);

            for (var i = 0; i < newPathSegments.Count; i++)
            {
                // Check if the pattern can be shortened because the remaining parameters are optional
                //
                // e.g. Matching pattern {controller=Home}/{action=Index} against HomeController.Index
                // can resolve to the following endpoints: (sorted by RouteEndpoint.Order)
                // - /
                // - /Home
                // - /Home/Index
                if (UseDefaultValuePlusRemainingSegmentsOptional(
                    i,
                    resolvedRouteValues,
                    pattern.Defaults,
                    ref nonInlineDefaultsCopy,
                    newPathSegments))
                {
                    // The route pattern has matching default values AND an optional parameter
                    // For link generation we need to include an endpoint with parameters and default values
                    // so the link is correctly shortened
                    // e.g. {controller=Home}/{action=Index}/{id=17}
                    if (!hasLinkGenerationEndpoint)
                    {
                        _models.Add(CreateModel(
                            model,
                            order++,
                            name,
                            GetPattern(newPathSegments),
                            nonInlineDefaultsCopy,
                            constraints,
                            dataTokens,
                            newPathSegments,
                            resolvedRouteValues,
                            suppressLinkGeneration,
                            suppressPathMatching: true));

                        hasLinkGenerationEndpoint = true;
                    }

                    var subPathSegments = newPathSegments.Take(i);

                    _models.Add(CreateModel(
                        model,
                        order++,
                        name,
                        GetPattern(subPathSegments),
                        nonInlineDefaultsCopy,
                        constraints,
                        dataTokens,
                        subPathSegments,
                        resolvedRouteValues,
                        suppressLinkGeneration,
                        suppressPathMatching));
                }

                UpdatePathSegments(i, resolvedRouteValues, pattern, parameterPolicies, newPathSegments);
            }

            _models.Add(CreateModel(
                model,
                order++,
                name,
                GetPattern(newPathSegments),
                constraints,
                nonInlineDefaultsCopy,
                dataTokens,
                newPathSegments,
                resolvedRouteValues,
                suppressLinkGeneration,
                suppressPathMatching));

            return order;

            string GetPattern(IEnumerable<RoutePatternPathSegment> segments)
            {
                var sb = GetTemplateBuilder();

                RoutePatternWriter.WriteString(sb, segments);
                var rawPattern = sb.ToString();
                sb.Length = 0;

                return rawPattern;
            }
        }

        private ControllerActionEndpointModel CreateModel(
            ControllerActionEndpointModel model,
            int order,
            string name, 
            string pattern, 
            IDictionary<string, object> defaults,
            object constraints,
            object dataTokens,
            IEnumerable<RoutePatternPathSegment> segments,
            IDictionary<string, string> requiredValues,
            bool suppressLinkGeneration,
            bool suppressPathMatching)
        {
            model = new ControllerActionEndpointModel(model)
            {
                Order = order,
                RouteName = name,
                RoutePattern = RoutePatternFactory.Pattern(pattern, defaults, constraints, segments),
            };

            return model;
        }

        private static IDictionary<string, string> ResolveActionRouteValues(ControllerActionEndpointModel model, IReadOnlyDictionary<string, object> allDefaults)
        {
            Dictionary<string, string> resolvedRequiredValues = null;

            foreach (var kvp in model.RequiredValues)
            {
                // Check whether there is a matching default value with a different case
                // e.g. {controller=HOME}/{action} with HomeController.Index will have route values:
                // - controller = HOME
                // - action = Index
                if (allDefaults.TryGetValue(kvp.Key, out var value) &&
                    value is string defaultValue &&
                    !string.Equals(kvp.Value, defaultValue, StringComparison.Ordinal) &&
                    string.Equals(kvp.Value, defaultValue, StringComparison.OrdinalIgnoreCase))
                {
                    if (resolvedRequiredValues == null)
                    {
                        resolvedRequiredValues = new Dictionary<string, string>(model.RequiredValues, StringComparer.OrdinalIgnoreCase);
                    }

                    resolvedRequiredValues[kvp.Key] = defaultValue;
                }
            }

            return resolvedRequiredValues ?? model.RequiredValues;
        }

        private void UpdatePathSegments(
            int i,
            IDictionary<string, string> resolvedRequiredValues,
            RoutePattern routePattern,
            IDictionary<string, IList<IParameterPolicy>> parameterPolicies,
            List<RoutePatternPathSegment> newPathSegments)
        {
            List<RoutePatternPart> segmentParts = null; // Initialize only as needed
            var segment = newPathSegments[i];
            for (var j = 0; j < segment.Parts.Count; j++)
            {
                var part = segment.Parts[j];

                if (part is RoutePatternParameterPart parameterPart)
                {
                    if (resolvedRequiredValues.TryGetValue(parameterPart.Name, out var parameterRouteValue))
                    {
                        if (segmentParts == null)
                        {
                            segmentParts = segment.Parts.ToList();
                        }

                        // Route value could be null if it is a "known" route value.
                        // Do not use the null value to de-normalize the route pattern,
                        // instead leave the parameter unchanged.
                        // e.g.
                        //     RouteValues will contain a null "page" value if there are Razor pages
                        //     Skip replacing the {page} parameter
                        if (parameterRouteValue != null)
                        {
                            if (parameterPolicies.TryGetValue(parameterPart.Name, out var policies))
                            {
                                // Check if the parameter has a transformer policy
                                // Use the first transformer policy
                                for (var k = 0; k < policies.Count; k++)
                                {
                                    if (policies[k] is IOutboundParameterTransformer parameterTransformer)
                                    {
                                        parameterRouteValue = parameterTransformer.TransformOutbound(parameterRouteValue);
                                        break;
                                    }
                                }
                            }

                            segmentParts[j] = RoutePatternFactory.LiteralPart(parameterRouteValue);
                        }
                    }
                }
            }

            // A parameter part was replaced so replace segment with updated parts
            if (segmentParts != null)
            {
                newPathSegments[i] = RoutePatternFactory.Segment(segmentParts);
            }
        }

        private bool UseDefaultValuePlusRemainingSegmentsOptional(
            int segmentIndex,
            IDictionary<string, string> resolvedRequiredValues,
            IReadOnlyDictionary<string, object> allDefaults,
            ref RouteValueDictionary nonInlineDefaults,
            List<RoutePatternPathSegment> pathSegments)
        {
            // Check whether the remaining segments are all optional and one or more of them is
            // for area/controller/action and has a default value
            var usedDefaultValue = false;

            for (var i = segmentIndex; i < pathSegments.Count; i++)
            {
                var segment = pathSegments[i];
                for (var j = 0; j < segment.Parts.Count; j++)
                {
                    var part = segment.Parts[j];
                    if (part.IsParameter && part is RoutePatternParameterPart parameterPart)
                    {
                        if (allDefaults.TryGetValue(parameterPart.Name, out var v))
                        {
                            if (resolvedRequiredValues.TryGetValue(parameterPart.Name, out var routeValue))
                            {
                                if (string.Equals(v as string, routeValue, StringComparison.OrdinalIgnoreCase))
                                {
                                    usedDefaultValue = true;
                                    continue;
                                }
                            }
                            else
                            {
                                if (nonInlineDefaults == null)
                                {
                                    nonInlineDefaults = new RouteValueDictionary();
                                }
                                nonInlineDefaults.TryAdd(parameterPart.Name, v);

                                usedDefaultValue = true;
                                continue;
                            }
                        }

                        if (parameterPart.IsOptional || parameterPart.IsCatchAll)
                        {
                            continue;
                        }
                    }
                    else if (part.IsSeparator && part is RoutePatternSeparatorPart separatorPart
                        && separatorPart.Content == ".")
                    {
                        // Check if this pattern ends in an optional extension, e.g. ".{ext?}"
                        // Current literal must be "." and followed by a single optional parameter part
                        var nextPartIndex = j + 1;

                        if (nextPartIndex == segment.Parts.Count - 1
                            && segment.Parts[nextPartIndex].IsParameter
                            && segment.Parts[nextPartIndex] is RoutePatternParameterPart extensionParameterPart
                            && extensionParameterPart.IsOptional)
                        {
                            continue;
                        }
                    }

                    // Stop because there is a non-optional/non-defaulted trailing value
                    return false;
                }
            }

            return usedDefaultValue;
        }

        private bool MatchRouteValue(
            IReadOnlyDictionary<string, string> requiredValues, 
            RoutePattern pattern,
            RouteValueDictionary defaults,
            Dictionary<string, IList<IParameterPolicy>> parameterPolicies,
            string routeKey)
        {
            if (!requiredValues.TryGetValue(routeKey, out var actionValue) || string.IsNullOrWhiteSpace(actionValue))
            {
                // Action does not have a value for this routeKey, most likely because action is not in an area
                // Check that the pattern does not have a parameter for the routeKey
                var matchingParameter = pattern.GetParameter(routeKey);
                if (matchingParameter == null &&
                    (!pattern.Defaults.TryGetValue(routeKey, out var value) ||
                    !string.IsNullOrEmpty(Convert.ToString(value))))
                {
                    return true;
                }
            }
            else
            {
                if (pattern.Defaults.TryGetValue(routeKey, out var defaultValue) &&
                    string.Equals(actionValue, defaultValue as string, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var matchingParameter = pattern.GetParameter(routeKey);
                if (matchingParameter != null)
                {
                    // Check that the value matches against constraints on that parameter
                    // e.g. For {controller:regex((Home|Login))} the controller value must match the regex
                    if (parameterPolicies.TryGetValue(routeKey, out var policies))
                    {
                        foreach (var policy in policies)
                        {
                            if (policy is IRouteConstraint constraint
                                && !constraint.Match(httpContext: null, NullRouter.Instance, routeKey, new RouteValueDictionary(requiredValues), RouteDirection.IncomingRequest))
                            {
                                // Did not match constraint
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        // Ensure route values are a subset of defaults
        // Examples:
        //
        // Template: {controller}/{action}/{category}/{id?}
        // Defaults(in-line or non in-line): category=products
        // Required values: controller=foo, action=bar
        // Final constructed pattern: foo/bar/{category}/{id?}
        // Final defaults: controller=foo, action=bar, category=products
        //
        // Template: {controller=Home}/{action=Index}/{category=products}/{id?}
        // Defaults: controller=Home, action=Index, category=products
        // Required values: controller=foo, action=bar
        // Final constructed pattern: foo/bar/{category}/{id?}
        // Final defaults: controller=foo, action=bar, category=products
        private void EnsureRequiredValuesInDefaults(
            IDictionary<string, string> routeValues,
            RouteValueDictionary defaults,
            IEnumerable<RoutePatternPathSegment> segments)
        {
            foreach (var kvp in routeValues)
            {
                if (kvp.Value != null)
                {
                    defaults[kvp.Key] = kvp.Value;
                }
            }
        }

        // This is safe because this is always used within the lock.
        private StringBuilder GetTemplateBuilder()
        {
            if (_templateBuilder == null)
            {
                _templateBuilder = new StringBuilder();
            }

            _templateBuilder.Clear();
            return _templateBuilder;
        }
    }
}
