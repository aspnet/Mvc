﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    internal static class ActionAttributeRouteModel
    {
        public static IEnumerable<SelectorModel> FlattenSelectors(ActionModel actionModel)
        {
            // Loop through all attribute routes defined on the controller. 
            // These perform a cross-product with all of the action-level attribute routes.
            var controllerSelectors = actionModel.Controller.Selectors
                .Where(sm => sm.AttributeRouteModel != null)
                .ToList();

            // We also include metadata and action constraints from the controller
            // even when there are no routes, or when an action overrides the route template.
            SelectorModel additionalSelector = null;
            if (actionModel.Controller.Selectors.Count > 0)
            {
                // This logic seems arbitrary but there's a good reason for it.
                // 
                // When we build the controller level selectors, any metadata or action constraints
                // that aren't IRouteTemplateProvider will be included in all selectors. So we 
                // pick any selector and then grab all of the stuff that isn't IRouteTemplateProvider
                // then we've found all of the items that aren't routes.
                //
                // This is fragile wrt application model customizing the data - but no one has
                // run into an issue with this and its pretty esoteric.
                additionalSelector = new SelectorModel(actionModel.Controller.Selectors.First());
                additionalSelector.AttributeRouteModel = null;

                for (var i = additionalSelector.ActionConstraints.Count - 1; i >= 0; i--)
                {
                    if (additionalSelector.ActionConstraints[i] is IRouteTemplateProvider)
                    {
                        additionalSelector.ActionConstraints.RemoveAt(i);
                    }
                }

                for (var i = additionalSelector.EndpointMetadata.Count - 1; i >= 0; i--)
                {
                    if (additionalSelector.EndpointMetadata[i] is IRouteTemplateProvider)
                    {
                        additionalSelector.EndpointMetadata.RemoveAt(i);
                    }
                }
            }

            var actionConstraints = new List<IActionConstraintMetadata>();

            foreach (var actionSelector in actionModel.Selectors)
            {
                var actionRouteModel = actionSelector.AttributeRouteModel;

                // We check the action to see if the template allows combination behavior
                // (It doesn't start with / or ~/) so that in the case where we have multiple
                // [Route] attributes on the controller we don't end up creating multiple
                if (actionRouteModel != null && actionRouteModel.IsAbsoluteTemplate)
                {
                    // We're overriding the routes from the controller, but any *unbound* constraints
                    // still apply.
                    var selector = new SelectorModel(actionSelector);

                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        left: null,
                        right: actionRouteModel);

                    AddActionConstraints(selector, additionalSelector?.ActionConstraints);
                    AddEndpointMetadata(selector, additionalSelector?.EndpointMetadata);

                    yield return selector;
                }
                else if (controllerSelectors.Count > 0)
                {
                    for (var i = 0; i < controllerSelectors.Count; i++)
                    {
                        var controllerSelector = controllerSelectors[i];

                        // We're using the attribute routes from the controller
                        var selector = new SelectorModel(actionSelector);

                        selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                            controllerSelector.AttributeRouteModel,
                            actionRouteModel);

                        AddActionConstraints(selector, controllerSelector.ActionConstraints);
                        AddEndpointMetadata(selector, controllerSelector.EndpointMetadata);

                        // No need to include the additional selector here because it would duplicate
                        // data in controllerSelector.

                        yield return selector;
                    }
                }
                else
                {
                    // There are no routes on the controller, but any *unbound* constraints
                    // still apply.
                    var selector = new SelectorModel(actionSelector);

                    selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                        left: null,
                        right: actionRouteModel);

                    AddActionConstraints(selector, additionalSelector?.ActionConstraints);
                    AddEndpointMetadata(selector, additionalSelector?.EndpointMetadata);

                    yield return selector;
                }
            }
        }

        private static void AddActionConstraints(SelectorModel selector, IList<IActionConstraintMetadata> actionConstraints)
        {
            if (actionConstraints != null)
            {
                for (var i = 0; i < actionConstraints.Count;i++)
                {
                    selector.ActionConstraints.Add(actionConstraints[i]);
                }
            }
        }

        private static void AddEndpointMetadata(SelectorModel selector, IList<object> metadata)
        {
            if (metadata != null)
            {
                for (var i = 0; i < metadata.Count; i++)
                {
                    selector.EndpointMetadata.Add(metadata[i]);
                }
            }
        }

        public static IEnumerable<(AttributeRouteModel route, SelectorModel actionSelector, SelectorModel controllerSelector)> GetAttributeRoutes(ActionModel actionModel)
        {
            var controllerAttributeRoutes = actionModel.Controller.Selectors
                .Where(sm => sm.AttributeRouteModel != null)
                .Select(sm => sm.AttributeRouteModel)
                .ToList();

            foreach (var actionSelectorModel in actionModel.Selectors)
            {
                var actionRouteModel = actionSelectorModel.AttributeRouteModel;

                // We check the action to see if the template allows combination behavior
                // (It doesn't start with / or ~/) so that in the case where we have multiple
                // [Route] attributes on the controller we don't end up creating multiple
                if (actionRouteModel != null && actionRouteModel.IsAbsoluteTemplate)
                {
                    var route = AttributeRouteModel.CombineAttributeRouteModel(
                        left: null,
                        right: actionRouteModel);

                    yield return (route, actionSelectorModel, null);
                }
                else if (controllerAttributeRoutes.Count > 0)
                {
                    for (var i = 0; i < actionModel.Controller.Selectors.Count; i++)
                    {
                        // We're using the attribute routes from the controller
                        var controllerSelector = actionModel.Controller.Selectors[i];

                        var route = AttributeRouteModel.CombineAttributeRouteModel(
                            controllerSelector.AttributeRouteModel,
                            actionRouteModel);

                        yield return (route, actionSelectorModel, controllerSelector);
                    }
                }

                else
                {
                    var route = AttributeRouteModel.CombineAttributeRouteModel(
                        left: null,
                        right: actionRouteModel);

                    yield return (route, actionSelectorModel, null);
                }
            }
        }
    }
}
