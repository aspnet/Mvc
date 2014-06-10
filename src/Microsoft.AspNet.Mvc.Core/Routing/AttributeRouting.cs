// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.Routing.Template;
using Microsoft.AspNet.Routing;

namespace Microsoft.AspNet.Mvc
{
    public static class AttributeRouting
    {
        // Key used by routing and action selection to match an attribute route entry to a
        // group of action descriptors.
        public readonly static string RouteGroupKey = "!__route_group";

        /// <summary>
        /// Creates an attribute route using the provided services and provided target router.
        /// </summary>
        /// <param name="target">The router to invoke an a route entry matches.</param>
        /// <param name="services">The application services.</param>
        /// <returns>An attribute route.</returns>
        public static IRouter CreateAttributeRoute([NotNull] IRouter target, [NotNull] IServiceProvider services)
        {
            var actions = GetActionDescriptors(services);

            // We're creating one AttributeRouteEntry per group, so we need to identify the distinct set of
            // groups. It's guaranteed that all members of the group have the same template and precedence,
            // so we only need to hang on to a single instance of 'action.RouteInfo' per-group.
            var routeTemplatesByGroup = new Dictionary<string, RouteInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var action in actions.Where(a => a.RouteInfo != null))
            {
                if (!routeTemplatesByGroup.ContainsKey(action.RouteInfo.RouteGroup))
                {
                    routeTemplatesByGroup.Add(action.RouteInfo.RouteGroup, action.RouteInfo);
                }
            }

            var inlineConstraintResolver = services.GetService<IInlineConstraintResolver>();

            var entries = new List<AttributeRouteEntry>();
            foreach (var entry in routeTemplatesByGroup)
            {
                var routeToken = entry.Key;

                entries.Add(new AttributeRouteEntry()
                {
                    Precedence = entry.Value.Precedence,
                    Route = new TemplateRoute(
                        target,
                        entry.Value.TemplateText,
                        defaults: new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                        {
                            { RouteGroupKey, routeToken },
                        },
                        constraints: null,
                        inlineConstraintResolver: inlineConstraintResolver),
                });
            }

            return new AttributeRoute(target, entries);
        }

        private static IReadOnlyList<ActionDescriptor> GetActionDescriptors(IServiceProvider services)
        {
            var actionDescriptorProvider = services.GetService<IActionDescriptorsCollectionProvider>();

            var actionDescriptorsCollection = actionDescriptorProvider.ActionDescriptors;
            return actionDescriptorsCollection.Items;
        }
    }
}