// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ReflectedModelBuilder;
using Microsoft.AspNet.Mvc.Routing;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionDescriptorProvider : IActionDescriptorProvider
    {
        /// <summary>
        /// Represents the default order associated with this provider for dependency injection
        /// purposes.
        /// </summary>
        public static readonly int DefaultOrder = 0;

        // This is the default order for attribute routes whose order calculated from
        // the reflected model is null.
        private const int DefaultAttributeRouteOrder = 0;

        private readonly IControllerAssemblyProvider _controllerAssemblyProvider;
        private readonly IActionDiscoveryConventions _conventions;
        private readonly IEnumerable<IFilter> _globalFilters;
        private readonly IEnumerable<IReflectedApplicationModelConvention> _modelConventions;
        private readonly IInlineConstraintResolver _constraintResolver;

        public ReflectedActionDescriptorProvider(IControllerAssemblyProvider controllerAssemblyProvider,
                                                 IActionDiscoveryConventions conventions,
                                                 IEnumerable<IFilter> globalFilters,
                                                 IOptionsAccessor<MvcOptions> optionsAccessor,
                                                 IInlineConstraintResolver constraintResolver)
        {
            _controllerAssemblyProvider = controllerAssemblyProvider;
            _conventions = conventions;
            _globalFilters = globalFilters ?? Enumerable.Empty<IFilter>();
            _modelConventions = optionsAccessor.Options.ApplicationModelConventions;
            _constraintResolver = constraintResolver;
        }

        public int Order
        {
            get { return DefaultOrder; }
        }

        public void Invoke(ActionDescriptorProviderContext context, Action callNext)
        {
            context.Results.AddRange(GetDescriptors());
            callNext();
        }

        public IEnumerable<ReflectedActionDescriptor> GetDescriptors()
        {
            var model = BuildModel();

            foreach (var convention in _modelConventions)
            {
                convention.OnModelCreated(model);
            }

            return Build(model);
        }

        public ReflectedApplicationModel BuildModel()
        {
            var applicationModel = new ReflectedApplicationModel();
            applicationModel.Filters.AddRange(_globalFilters);

            var assemblies = _controllerAssemblyProvider.CandidateAssemblies;
            var types = assemblies.SelectMany(a => a.DefinedTypes);
            var controllerTypes = types.Where(_conventions.IsController);

            foreach (var controllerType in controllerTypes)
            {
                var controllerModel = new ReflectedControllerModel(controllerType);
                applicationModel.Controllers.Add(controllerModel);

                foreach (var methodInfo in controllerType.AsType().GetMethods())
                {
                    var actionInfos = _conventions.GetActions(methodInfo, controllerType);
                    if (actionInfos == null)
                    {
                        continue;
                    }

                    foreach (var actionInfo in actionInfos)
                    {
                        var actionModel = new ReflectedActionModel(methodInfo);

                        actionModel.ActionName = actionInfo.ActionName;
                        actionModel.IsActionNameMatchRequired = actionInfo.RequireActionNameMatch;
                        actionModel.HttpMethods.AddRange(actionInfo.HttpMethods ?? Enumerable.Empty<string>());

                        if (actionInfo.AttributeRoute != null)
                        {
                            actionModel.AttributeRouteModel = new ReflectedAttributeRouteModel(
                                actionInfo.AttributeRoute);
                        }

                        foreach (var parameter in methodInfo.GetParameters())
                        {
                            actionModel.Parameters.Add(new ReflectedParameterModel(parameter));
                        }

                        controllerModel.Actions.Add(actionModel);
                    }
                }
            }

            return applicationModel;
        }

        public List<ReflectedActionDescriptor> Build(ReflectedApplicationModel model)
        {
            var actions = new List<ReflectedActionDescriptor>();

            var hasAttributeRoutes = false;
            var removalConstraints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var methodInfoMap =
                new Dictionary<MethodInfo,
                               IDictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>>>();

            var routeTemplateErrors = new List<string>();
            var attributeRoutingConfigurationErrors = new Dictionary<MethodInfo, string>();

            foreach (var controller in model.Controllers)
            {
                var controllerDescriptor = new ControllerDescriptor(controller.ControllerType);
                foreach (var action in controller.Actions)
                {
                    // Controllers with multiple [Route] attributes (or user defined implementation of
                    // IRouteTemplateProvider) will generate one action descriptor per IRouteTemplateProvider
                    // instance.
                    // Actions with multiple [Http*] attributes or other (IRouteTemplateProvider implementations
                    // have already been identified as different actions during action discovery.
                    var actionDescriptors = CreateActionDescriptors(action, controller, controllerDescriptor);

                    foreach (var actionDescriptor in actionDescriptors)
                    {
                        AddActionFilters(actionDescriptor, action.Filters, controller.Filters, model.Filters);
                        AddActionConstraints(actionDescriptor, action, controller);
                        AddControllerRouteConstraints(
                            actionDescriptor,
                            controller.RouteConstraints,
                            removalConstraints);

                        if (IsAttributeRoutedAction(actionDescriptor))
                        {
                            hasAttributeRoutes = true;

                            // An attribute routed action will ignore conventional routed constraints. We still
                            // want to provide these values as ambient values.
                            AddConstraintsAsDefaultRouteValues(actionDescriptor);

                            // Replaces tokens like [controller]/[action] in the route template with the actual values
                            // for this action.
                            ReplaceAttributeRouteTokens(actionDescriptor, routeTemplateErrors);

                            // Attribute routed actions will ignore conventional routed constraints. Instead they have
                            // a single route constraint "RouteGroup" associated with it.
                            ReplaceRouteConstraints(actionDescriptor);
                        }
                    }

                    // Filter duplicate action descriptors, that is, those action descriptors that have the same
                    // attribute route template and the same method info. This means that for example, the
                    // combination of [Route]+[HttpGet] produced the same attribute route template for two action
                    // descriptors on the same C# method. For example, [Route("Products")]+"[HttpGet("{id}")] and
                    // [HttpGet("/Products/{id}")].
                    // We also update the methodInfoMap by adding the given action and action descriptors to the map.
                    actionDescriptors = FilterDuplicates(methodInfoMap, action, actionDescriptors);

                    actions.AddRange(actionDescriptors);
                }
            }

            var actionsByRouteName = new Dictionary<string, IList<ActionDescriptor>>(
                StringComparer.OrdinalIgnoreCase);

            // Keeps track of all the C# methods that we've validated to avoid visiting each action group
            // more than once.
            var validatedMethods = new HashSet<MethodInfo>();

            foreach (var actionDescriptor in actions)
            {
                if (!validatedMethods.Contains(actionDescriptor.MethodInfo))
                {
                    ValidateActionGroupConfiguration(
                        methodInfoMap,
                        actionDescriptor,
                        attributeRoutingConfigurationErrors);

                    validatedMethods.Add(actionDescriptor.MethodInfo);
                }

                if (!IsAttributeRoutedAction(actionDescriptor))
                {
                    // Any attribute routes are in use, then non-attribute-routed action descriptors can't be
                    // selected when a route group returned by the route.
                    if (hasAttributeRoutes)
                    {
                        actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                            AttributeRouting.RouteGroupKey,
                            RouteKeyHandling.DenyKey));
                    }

                    // Add a route constraint with DenyKey for each constraint in the set to all the
                    // actions that don't have that constraint. For example, if a controller defines
                    // an area constraint, all actions that don't belong to an area must have a route
                    // constraint that prevents them from matching an incomming request.
                    AddRemovalConstraints(actionDescriptor, removalConstraints);
                }
                else
                {
                    var attributeRouteInfo = actionDescriptor.AttributeRouteInfo;
                    if (attributeRouteInfo.Name != null)
                    {
                        // Build a map of attribute route name to action descriptors to ensure that all
                        // attribute routes with a given name have the same template.
                        AddActionToNamedGroup(actionsByRouteName, attributeRouteInfo.Name, actionDescriptor);
                    }

                    // We still want to add a 'null' for any constraint with DenyKey so that link generation
                    // works properly.
                    //
                    // Consider an action like { area = "", controller = "Home", action = "Index" }. Even if
                    // it's attribute routed, it needs to know that area must be null to generate a link.
                    foreach (var key in removalConstraints)
                    {
                        if (!actionDescriptor.RouteValueDefaults.ContainsKey(key))
                        {
                            actionDescriptor.RouteValueDefaults.Add(key, null);
                        }
                    }
                }
            }

            if (attributeRoutingConfigurationErrors.Any())
            {
                var message = CreateAttributeRoutingAggregateErrorMessage(
                    attributeRoutingConfigurationErrors.Values);

                throw new InvalidOperationException(message);
            }

            var namedRoutedErrors = ValidateNamedAttributeRoutedActions(actionsByRouteName);
            if (namedRoutedErrors.Any())
            {
                var message = CreateAttributeRoutingAggregateErrorMessage(namedRoutedErrors);
                throw new InvalidOperationException(message);
            }

            if (routeTemplateErrors.Any())
            {
                var message = CreateAttributeRoutingAggregateErrorMessage(routeTemplateErrors);
                throw new InvalidOperationException(message);
            }

            return actions;
        }

        private static IList<ReflectedActionDescriptor> CreateActionDescriptors(
            ReflectedActionModel action,
            ReflectedControllerModel controller,
            ControllerDescriptor controllerDescriptor)
        {
            var actionDescriptors = new List<ReflectedActionDescriptor>();

            if (controller.AttributeRoutes != null &&
                controller.AttributeRoutes.Count > 0)
            {
                foreach (var controllerAttributeRoute in controller.AttributeRoutes)
                {
                    var actionDescriptor = CreateActionDescriptor(
                        action,
                        controllerAttributeRoute,
                        controllerDescriptor);

                    actionDescriptors.Add(actionDescriptor);
                }
            }
            else
            {
                actionDescriptors.Add(CreateActionDescriptor(
                    action,
                    controllerAttributeRoute: null,
                    controllerDescriptor: controllerDescriptor));
            }

            return actionDescriptors;
        }

        private static ReflectedActionDescriptor CreateActionDescriptor(
            ReflectedActionModel action,
            ReflectedAttributeRouteModel controllerAttributeRoute,
            ControllerDescriptor controllerDescriptor)
        {
            var parameterDescriptors = new List<ParameterDescriptor>();
            foreach (var parameter in action.Parameters)
            {
                var isFromBody = parameter.Attributes.OfType<FromBodyAttribute>().Any();

                parameterDescriptors.Add(new ParameterDescriptor()
                {
                    Name = parameter.ParameterName,
                    IsOptional = parameter.IsOptional,

                    ParameterBindingInfo = isFromBody
                        ? null
                        : new ParameterBindingInfo(
                            parameter.ParameterName,
                            parameter.ParameterInfo.ParameterType),

                    BodyParameterInfo = isFromBody
                        ? new BodyParameterInfo(parameter.ParameterInfo.ParameterType)
                        : null
                });
            }

            var attributeRouteInfo = CreateAttributeRouteInfo(
                action.AttributeRouteModel,
                controllerAttributeRoute);

            var actionDescriptor = new ReflectedActionDescriptor()
            {
                Name = action.ActionName,
                ControllerDescriptor = controllerDescriptor,
                MethodInfo = action.ActionMethod,
                Parameters = parameterDescriptors,
                RouteConstraints = new List<RouteDataActionConstraint>(),
                AttributeRouteInfo = attributeRouteInfo
            };

            actionDescriptor.DisplayName = string.Format(
                "{0}.{1}",
                action.ActionMethod.DeclaringType.FullName,
                action.ActionMethod.Name);

            return actionDescriptor;
        }

        private List<ReflectedActionDescriptor> FilterDuplicates(
            IDictionary<MethodInfo, IDictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>>> methodMap,
            ReflectedActionModel action,
            IList<ReflectedActionDescriptor> actionDescriptors)
        {
            var filteredList = new List<ReflectedActionDescriptor>();

            IDictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>> actionsForMethod = null;
            if (methodMap.TryGetValue(action.ActionMethod, out actionsForMethod))
            {
                var actions = actionsForMethod.SelectMany(a => a.Value).ToArray();

                foreach (var descriptor in actionDescriptors)
                {
                    if (!IsDuplicateActionDescriptor(actions, descriptor))
                    {
                        filteredList.Add(descriptor);
                    }
                }

                actionsForMethod.Add(action, actionDescriptors);
            }
            else
            {
                var reflectedActionMap = new Dictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>>();
                reflectedActionMap.Add(action, actionDescriptors);
                methodMap.Add(action.ActionMethod, reflectedActionMap);

                foreach (var actionDescriptor in actionDescriptors)
                {
                    if (!IsDuplicateActionDescriptor(filteredList, actionDescriptor))
                    {
                        filteredList.Add(actionDescriptor);
                    }
                }
            }

            return filteredList;
        }

        private static bool IsDuplicateActionDescriptor(
            IEnumerable<ReflectedActionDescriptor> actions,
            ReflectedActionDescriptor descriptor)
        {
            return actions.Any(a => a.AttributeRouteInfo != null &&
                                    a.AttributeRouteInfo.Template != null &&
                                    descriptor.AttributeRouteInfo != null &&
                                    a.AttributeRouteInfo.Template.Equals(
                                        descriptor.AttributeRouteInfo.Template,
                                        StringComparison.OrdinalIgnoreCase));
        }

        private static void AddActionFilters(
            ReflectedActionDescriptor actionDescriptor,
            IEnumerable<IFilter> actionFilters,
            IEnumerable<IFilter> controllerFilters,
            IEnumerable<IFilter> globalFilters)
        {
            actionDescriptor.FilterDescriptors =
                            actionFilters.Select(f => new FilterDescriptor(f, FilterScope.Action))
                            .Concat(controllerFilters.Select(f => new FilterDescriptor(f, FilterScope.Controller)))
                            .Concat(globalFilters.Select(f => new FilterDescriptor(f, FilterScope.Global)))
                            .OrderBy(d => d, FilterDescriptorOrderComparer.Comparer)
                            .ToList();
        }

        private static AttributeRouteInfo CreateAttributeRouteInfo(
            ReflectedAttributeRouteModel action,
            ReflectedAttributeRouteModel controller)
        {
            var combinedRoute = ReflectedAttributeRouteModel.CombineReflectedAttributeRouteModel(
                                controller,
                                action);

            return combinedRoute == null ? null : new AttributeRouteInfo()
            {
                Template = combinedRoute.Template,
                Order = combinedRoute.Order ?? DefaultAttributeRouteOrder,
                Name = combinedRoute.Name,
            };
        }

        private static void AddActionConstraints(
            ReflectedActionDescriptor actionDescriptor,
            ReflectedActionModel action,
            ReflectedControllerModel controller)
        {
            var httpMethods = action.HttpMethods;
            if (httpMethods != null && httpMethods.Count > 0)
            {
                actionDescriptor.MethodConstraints = new List<HttpMethodConstraint>()
                        {
                            new HttpMethodConstraint(httpMethods)
                        };
            }

            actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                "controller",
                controller.ControllerName));

            if (action.IsActionNameMatchRequired)
            {
                actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                    "action",
                    action.ActionName));
            }
            else
            {
                actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                    "action",
                    RouteKeyHandling.DenyKey));
            }
        }

        private static void AddControllerRouteConstraints(
            ReflectedActionDescriptor actionDescriptor,
            IList<RouteConstraintAttribute> routeconstraints,
            ISet<string> removalConstraints)
        {
            foreach (var constraintAttribute in routeconstraints)
            {
                if (constraintAttribute.BlockNonAttributedActions)
                {
                    removalConstraints.Add(constraintAttribute.RouteKey);
                }

                // Skip duplicates
                if (!HasConstraint(actionDescriptor.RouteConstraints, constraintAttribute.RouteKey))
                {
                    if (constraintAttribute.RouteValue == null)
                    {
                        actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                            constraintAttribute.RouteKey,
                            constraintAttribute.RouteKeyHandling));
                    }
                    else
                    {
                        actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                            constraintAttribute.RouteKey,
                            constraintAttribute.RouteValue));
                    }
                }
            }
        }

        private static bool HasConstraint(List<RouteDataActionConstraint> constraints, string routeKey)
        {
            return constraints.Any(
                rc => string.Equals(rc.RouteKey, routeKey, StringComparison.OrdinalIgnoreCase));
        }

        private static void ReplaceRouteConstraints(ReflectedActionDescriptor actionDescriptor)
        {
            var routeGroupValue = GetRouteGroupValue(
                actionDescriptor.AttributeRouteInfo.Order,
                actionDescriptor.AttributeRouteInfo.Template);

            var routeConstraints = new List<RouteDataActionConstraint>();
            routeConstraints.Add(new RouteDataActionConstraint(
                AttributeRouting.RouteGroupKey,
                routeGroupValue));

            actionDescriptor.RouteConstraints = routeConstraints;
        }

        private static void ReplaceAttributeRouteTokens(
            ReflectedActionDescriptor actionDescriptor,
            IList<string> routeTemplateErrors)
        {
            try
            {
                actionDescriptor.AttributeRouteInfo.Template = ReflectedAttributeRouteModel.ReplaceTokens(
                    actionDescriptor.AttributeRouteInfo.Template,
                    actionDescriptor.RouteValueDefaults);
            }
            catch (InvalidOperationException ex)
            {
                var message = Resources.FormatAttributeRoute_IndividualErrorMessage(
                    actionDescriptor.DisplayName,
                    Environment.NewLine,
                    ex.Message);

                routeTemplateErrors.Add(message);
            }
        }

        private static void AddConstraintsAsDefaultRouteValues(ReflectedActionDescriptor actionDescriptor)
        {
            foreach (var constraint in actionDescriptor.RouteConstraints)
            {
                // We don't need to do anything with attribute routing for 'catch all' behavior. Order
                // and predecedence of attribute routes allow this kind of behavior.
                if (constraint.KeyHandling == RouteKeyHandling.RequireKey ||
                    constraint.KeyHandling == RouteKeyHandling.DenyKey)
                {
                    actionDescriptor.RouteValueDefaults.Add(constraint.RouteKey, constraint.RouteValue);
                }
            }
        }

        private static void AddRemovalConstraints(
            ReflectedActionDescriptor actionDescriptor,
            ISet<string> removalConstraints)
        {
            foreach (var key in removalConstraints)
            {
                if (!HasConstraint(actionDescriptor.RouteConstraints, key))
                {
                    actionDescriptor.RouteConstraints.Add(new RouteDataActionConstraint(
                        key,
                        RouteKeyHandling.DenyKey));
                }
            }
        }

        private static void AddActionToNamedGroup(
            IDictionary<string, IList<ActionDescriptor>> actionsByRouteName,
            string routeName,
            ReflectedActionDescriptor actionDescriptor)
        {
            IList<ActionDescriptor> namedActionGroup;

            if (actionsByRouteName.TryGetValue(routeName, out namedActionGroup))
            {
                namedActionGroup.Add(actionDescriptor);
            }
            else
            {
                namedActionGroup = new List<ActionDescriptor>();
                namedActionGroup.Add(actionDescriptor);
                actionsByRouteName.Add(routeName, namedActionGroup);
            }
        }

        private static bool IsAttributeRoutedAction(ReflectedActionDescriptor actionDescriptor)
        {
            return actionDescriptor.AttributeRouteInfo != null &&
                actionDescriptor.AttributeRouteInfo.Template != null;
        }

        private static IList<string> AddErrorNumbers(
            IEnumerable<string> namedRoutedErrors)
        {
            return namedRoutedErrors
                .Select((nre, i) =>
                            Resources.FormatAttributeRoute_AggregateErrorMessage_ErrorNumber(
                                i + 1,
                                Environment.NewLine,
                                nre))
                .ToList();
        }

        private static IList<string> ValidateNamedAttributeRoutedActions(
            IDictionary<string,
            IList<ActionDescriptor>> actionsGroupedByRouteName)
        {
            var namedRouteErrors = new List<string>();

            foreach (var kvp in actionsGroupedByRouteName)
            {
                // We are looking for attribute routed actions that have the same name but
                // different route templates. We pick the first template of the group and
                // we compare it against the rest of the templates that have that same name
                // associated.
                // The moment we find one that is different we report the whole group to the
                // user in the error message so that he can see the different actions and the
                // different templates for a given named attribute route.
                var firstActionDescriptor = kvp.Value[0];
                var firstTemplate = firstActionDescriptor.AttributeRouteInfo.Template;

                for (var i = 1; i < kvp.Value.Count; i++)
                {
                    var otherActionDescriptor = kvp.Value[i];
                    var otherActionTemplate = otherActionDescriptor.AttributeRouteInfo.Template;

                    if (!firstTemplate.Equals(otherActionTemplate, StringComparison.OrdinalIgnoreCase))
                    {
                        var descriptions = kvp.Value.Select(ad =>
                            Resources.FormatAttributeRoute_DuplicateNames_Item(
                                ad.DisplayName,
                                ad.AttributeRouteInfo.Template));

                        var errorDescription = string.Join(Environment.NewLine, descriptions);
                        var message = Resources.FormatAttributeRoute_DuplicateNames(
                            kvp.Key,
                            Environment.NewLine,
                            errorDescription);

                        namedRouteErrors.Add(message);
                        break;
                    }
                }
            }

            return namedRouteErrors;
        }

        private void ValidateActionGroupConfiguration(
            IDictionary<MethodInfo, IDictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>>> methodMap,
            ReflectedActionDescriptor actionDescriptor,
            IDictionary<MethodInfo, string> routingConfigurationErrors)
        {
            // Text to show as the attribute route template for conventionally routed actions.
            const string NullTemplate = "(null)";

            string mixedRoutingTypesErrorMessage = null;
            string invalidHttpErrorMessage = null;

            var hasAttributeRoutedActions = false;
            var hasConventionallyRoutedActions = false;

            // Validate that no C# method result in attribute and non attribute actions at the same time.
            // This is for example the case when someone uses [HttpGet("Products")] and [HttpPost]
            // on the same C# method. We consider this an invalid configuration.
            // By design all the actions in a C# method are either attribute routed or conventionally
            // routed, but mixing attribute and non attributed actions in the same method is not allowed.
            var actionsForMethod = methodMap[actionDescriptor.MethodInfo];
            foreach (var action in actionsForMethod.SelectMany(a => a.Value))
            {
                if (IsAttributeRoutedAction(action))
                {
                    hasAttributeRoutedActions = true;
                }
                else
                {
                    hasConventionallyRoutedActions = true;
                }

                // If we have valid and invalid attribute routed actions we found an invalid configuration.
                if (hasAttributeRoutedActions && hasConventionallyRoutedActions)
                {
                    break;
                }
            }

            if (hasAttributeRoutedActions && hasConventionallyRoutedActions)
            {
                var actionDescriptions = actionsForMethod
                    .SelectMany(a => a.Value)
                    .Select(ad =>
                    Resources.FormatAttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod_Item(
                        ad.DisplayName,
                        ad.AttributeRouteInfo != null ? ad.AttributeRouteInfo.Template : NullTemplate));

                var methodFullName = string.Format("{0}.{1}",
                        actionDescriptor.MethodInfo.DeclaringType.FullName,
                        actionDescriptor.MethodInfo.Name);

                // Sample error message:
                // A method 'MyApplication.CustomerController.Index' must not define attributed actions and
                // non attributed actions at the same time:
                // Action: 'MyApplication.CustomerController.Index' - Template: 'Products'
                // Action: 'MyApplication.CustomerController.Index' - Template: '(null)'
                mixedRoutingTypesErrorMessage =
                    Resources.FormatAttributeRoute_MixedAttributeAndConventionallyRoutedActions_ForMethod(
                        methodFullName,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, actionDescriptions));
            }

            if (hasAttributeRoutedActions)
            {
                invalidHttpErrorMessage = ValidateHttpMethodProviderAttributes(actionDescriptor, actionsForMethod);
            }

            if (invalidHttpErrorMessage != null &&
                mixedRoutingTypesErrorMessage != null)
            {
                var errorMessage = string.Join(
                    Environment.NewLine,
                    mixedRoutingTypesErrorMessage,
                    invalidHttpErrorMessage);

                routingConfigurationErrors.Add(actionDescriptor.MethodInfo, errorMessage);
            }
            else if (invalidHttpErrorMessage != null || mixedRoutingTypesErrorMessage != null)
            {
                routingConfigurationErrors.Add(
                    actionDescriptor.MethodInfo,
                    mixedRoutingTypesErrorMessage ?? invalidHttpErrorMessage);
            }
        }

        private static string ValidateHttpMethodProviderAttributes(
            ReflectedActionDescriptor actionDescriptor,
            IDictionary<ReflectedActionModel, IList<ReflectedActionDescriptor>> actionsForMethod)
        {
            // Validates that no C# method that creates attribute routed actions and
            // also uses attributes that only constraint the set of HTTP methods, for example [AcceptVerbs].
            // This situation is considered to be an invalid configuration. By design only attribute routes
            // can define which HTTP methods their resulted action allows by implementing
            // IActionHttpMethodProvider. An example of such attributes is [HttpGet].
            var invalidActions = new Dictionary<ReflectedActionModel, IEnumerable<string>>();

            foreach (var action in actionsForMethod)
            {
                var invalidConstraintAttributes = action.Key.Attributes
                    .Where(attr => attr is IActionHttpMethodProvider &&
                                   !(attr is IRouteTemplateProvider))
                    .Select(attr => attr.GetType().FullName);

                if (invalidConstraintAttributes.Any())
                {
                    invalidActions.Add(action.Key, invalidConstraintAttributes);
                }
            }

            var messagesForMethodInfo = new List<string>();
            if (invalidActions.Any())
            {
                foreach (var invalidAction in invalidActions)
                {
                    var invalidAttributesList = string.Join(", ", invalidAction.Value);
                    foreach (var descriptor in actionsForMethod[invalidAction.Key])
                    {
                        var messageItem = Resources.FormatAttributeRoute_InvalidHttpConstraints_Item(
                            descriptor.DisplayName,
                            invalidAttributesList,
                            typeof(IActionHttpMethodProvider).FullName);

                        messagesForMethodInfo.Add(messageItem);
                    }
                }

                var methodFullName = string.Format("{0}.{1}",
                    actionDescriptor.MethodInfo.DeclaringType.FullName,
                    actionDescriptor.MethodInfo.Name);

                // Sample message:
                // A method 'MyApplication.CustomerController.Index' that defines attribute routed actions must
                // not contain attributes that implement 'Microsoft.AspNet.Mvc.IActionHttpMethodProvider'
                // and do not implement 'Microsoft.AspNet.Mvc.Routing.IRouteTemplateProvider':
                // Action 'MyApplication.CustomerController.Index' has 'Microsoft.AspNet.Mvc.AcceptVerbsAttribute'
                // invalid 'Microsoft.AspNet.Mvc.IActionHttpMethodProvider' attributes.
                return
                    Resources.FormatAttributeRoute_InvalidHttpConstraints(
                        methodFullName,
                        typeof(IActionHttpMethodProvider).FullName,
                        typeof(IRouteTemplateProvider).FullName,
                        Environment.NewLine,
                        string.Join(Environment.NewLine, messagesForMethodInfo));
            }

            return null;
        }

        private static string CreateAttributeRoutingAggregateErrorMessage(IEnumerable<string> individualErrors)
        {
            var errorMessages = AddErrorNumbers(individualErrors);

            var message = Resources.FormatAttributeRoute_AggregateErrorMessage(
                Environment.NewLine,
                string.Join(Environment.NewLine + Environment.NewLine, errorMessages));
            return message;
        }

        private static string GetRouteGroupValue(int order, string template)
        {
            var group = string.Format("{0}-{1}", order, template);
            return ("__route__" + group).ToUpperInvariant();
        }
    }
}
