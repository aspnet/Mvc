// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Mvc.Internal
{
    public class DefaultApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ICollection<IFilterMetadata> _globalFilters;

        public DefaultApplicationModelProvider(IOptions<MvcOptions> mvcOptionsAccessor)
        {
            _globalFilters = mvcOptionsAccessor.Value.Filters;
        }

        /// <inheritdoc />
        public int Order
        {
            get
            {
                return -1000;
            }
        }

        /// <inheritdoc />
        public virtual void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var filter in _globalFilters)
            {
                context.Result.Filters.Add(filter);
            }

            foreach (var controllerType in context.ControllerTypes)
            {
                var controllerModels = BuildControllerModels(controllerType);
                if (controllerModels != null)
                {
                    foreach (var controllerModel in controllerModels)
                    {
                        context.Result.Controllers.Add(controllerModel);
                        controllerModel.Application = context.Result;

                        foreach (var propertyHelper in PropertyHelper.GetProperties(controllerType.AsType()))
                        {
                            var propertyInfo = propertyHelper.Property;
                            var propertyModel = CreatePropertyModel(propertyInfo);
                            if (propertyModel != null)
                            {
                                propertyModel.Controller = controllerModel;
                                controllerModel.ControllerProperties.Add(propertyModel);
                            }
                        }

                        foreach (var methodInfo in controllerType.AsType().GetMethods())
                        {
                            var actionModels = BuildActionModels(controllerType, methodInfo);
                            if (actionModels != null)
                            {
                                foreach (var actionModel in actionModels)
                                {
                                    actionModel.Controller = controllerModel;
                                    controllerModel.Actions.Add(actionModel);

                                    foreach (var parameterInfo in actionModel.ActionMethod.GetParameters())
                                    {
                                        var parameterModel = CreateParameterModel(parameterInfo);
                                        if (parameterModel != null)
                                        {
                                            parameterModel.Action = actionModel;
                                            actionModel.Parameters.Add(parameterModel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public virtual void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            // Intentionally empty.
        }

        /// <summary>
        /// Creates the <see cref="ControllerModel"/> instances for the given controller <see cref="TypeInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The controller <see cref="TypeInfo"/>.</param>
        /// <returns>
        /// A set of <see cref="ControllerModel"/> instances for the given controller <see cref="TypeInfo"/> or
        /// <c>null</c> if the <paramref name="typeInfo"/> does not represent a controller.
        /// </returns>
        protected virtual IEnumerable<ControllerModel> BuildControllerModels(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            var controllerModel = CreateControllerModel(typeInfo);
            yield return controllerModel;
        }

        /// <summary>
        /// Creates a <see cref="ControllerModel"/> for the given <see cref="TypeInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <returns>A <see cref="ControllerModel"/> for the given <see cref="TypeInfo"/>.</returns>
        protected virtual ControllerModel CreateControllerModel(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            // For attribute routes on a controller, we want want to support 'overriding' routes on a derived
            // class. So we need to walk up the hierarchy looking for the first class to define routes.
            //
            // Then we want to 'filter' the set of attributes, so that only the effective routes apply.
            var currentTypeInfo = typeInfo;
            var objectTypeInfo = typeof(object).GetTypeInfo();

            IRouteTemplateProvider[] routeAttributes = null;

            do
            {
                routeAttributes = currentTypeInfo
                        .GetCustomAttributes(inherit: false)
                        .OfType<IRouteTemplateProvider>()
                        .ToArray();

                if (routeAttributes.Length > 0)
                {
                    // Found 1 or more route attributes.
                    break;
                }

                currentTypeInfo = currentTypeInfo.BaseType.GetTypeInfo();
            }
            while (currentTypeInfo != objectTypeInfo);

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = typeInfo.GetCustomAttributes(inherit: true).OfType<object>().ToArray();

            // This is fairly complicated so that we maintain referential equality between items in
            // ControllerModel.Attributes and ControllerModel.Attributes[*].Attribute.
            var filteredAttributes = new List<object>();
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider)
                {
                    // This attribute is a route-attribute, leave it out.
                }
                else
                {
                    filteredAttributes.Add(attribute);
                }
            }
            filteredAttributes.AddRange(routeAttributes);

            attributes = filteredAttributes.ToArray();

            var controllerModel = new ControllerModel(typeInfo, attributes);
            AddRange(
                controllerModel.AttributeRoutes, routeAttributes.Select(a => new AttributeRouteModel(a)));

            controllerModel.ControllerName =
                typeInfo.Name.EndsWith("Controller", StringComparison.OrdinalIgnoreCase) ?
                    typeInfo.Name.Substring(0, typeInfo.Name.Length - "Controller".Length) :
                    typeInfo.Name;

            AddRange(controllerModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());
            AddRange(controllerModel.Filters, attributes.OfType<IFilterMetadata>());
            AddRange(controllerModel.RouteConstraints, attributes.OfType<IRouteConstraintProvider>());

            var apiVisibility = attributes.OfType<IApiDescriptionVisibilityProvider>().FirstOrDefault();
            if (apiVisibility != null)
            {
                controllerModel.ApiExplorer.IsVisible = !apiVisibility.IgnoreApi;
            }

            var apiGroupName = attributes.OfType<IApiDescriptionGroupNameProvider>().FirstOrDefault();
            if (apiGroupName != null)
            {
                controllerModel.ApiExplorer.GroupName = apiGroupName.GroupName;
            }

            // Controllers can implement action filter and result filter interfaces. We add
            // a special delegating filter implementation to the pipeline to handle it.
            //
            // This is needed because filters are instantiated before the controller.
            if (typeof(IAsyncActionFilter).GetTypeInfo().IsAssignableFrom(typeInfo) ||
                typeof(IActionFilter).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                controllerModel.Filters.Add(new ControllerActionFilter());
            }
            if (typeof(IAsyncResultFilter).GetTypeInfo().IsAssignableFrom(typeInfo) ||
                typeof(IResultFilter).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                controllerModel.Filters.Add(new ControllerResultFilter());
            }

            return controllerModel;
        }

        /// <summary>
        /// Creates a <see cref="PropertyModel"/> for the given <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/>.</param>
        /// <returns>A <see cref="PropertyModel"/> for the given <see cref="PropertyInfo"/>.</returns>
        protected virtual PropertyModel CreatePropertyModel(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentNullException(nameof(propertyInfo));
            }

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = propertyInfo.GetCustomAttributes(inherit: true).OfType<object>().ToArray();
            var propertyModel = new PropertyModel(propertyInfo, attributes);
            var bindingInfo = BindingInfo.GetBindingInfo(attributes);

            propertyModel.BindingInfo = bindingInfo;
            propertyModel.PropertyName = propertyInfo.Name;

            return propertyModel;
        }


        /// <summary>
        /// Creates the <see cref="ControllerModel"/> instances for the given action <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="typeInfo">The controller <see cref="TypeInfo"/>.</param>
        /// <param name="methodInfo">The action <see cref="MethodInfo"/>.</param>
        /// <returns>
        /// A set of <see cref="ActionModel"/> instances for the given action <see cref="MethodInfo"/> or
        /// <c>null</c> if the <paramref name="methodInfo"/> does not represent an action.
        /// </returns>
        protected virtual IEnumerable<ActionModel> BuildActionModels(
            TypeInfo typeInfo,
            MethodInfo methodInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (!IsAction(typeInfo, methodInfo))
            {
                return Enumerable.Empty<ActionModel>();
            }

            // For attribute routes on a action, we want want to support 'overriding' routes on a
            // virtual method, but allow 'overriding'. So we need to walk up the hierarchy looking
            // for the first definition to define routes.
            //
            // Then we want to 'filter' the set of attributes, so that only the effective routes apply.
            var currentMethodInfo = methodInfo;

            IRouteTemplateProvider[] routeAttributes = null;

            while (true)
            {
                routeAttributes = currentMethodInfo
                        .GetCustomAttributes(inherit: false)
                        .OfType<IRouteTemplateProvider>()
                        .ToArray();

                if (routeAttributes.Length > 0)
                {
                    // Found 1 or more route attributes.
                    break;
                }

                // GetBaseDefinition returns 'this' when it gets to the bottom of the chain.
                var nextMethodInfo = currentMethodInfo.GetBaseDefinition();
                if (currentMethodInfo == nextMethodInfo)
                {
                    break;
                }

                currentMethodInfo = nextMethodInfo;
            }

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = methodInfo.GetCustomAttributes(inherit: true).OfType<object>().ToArray();

            // This is fairly complicated so that we maintain referential equality between items in
            // ActionModel.Attributes and ActionModel.Attributes[*].Attribute.
            var applicableAttributes = new List<object>();
            foreach (var attribute in attributes)
            {
                if (attribute is IRouteTemplateProvider)
                {
                    // This attribute is a route-attribute, leave it out.
                }
                else
                {
                    applicableAttributes.Add(attribute);
                }
            }
            applicableAttributes.AddRange(routeAttributes);

            attributes = applicableAttributes.ToArray();

            // Route attributes create multiple actions, we want to split the set of
            // attributes based on these so each action only has the attributes that affect it.
            //
            // The set of route attributes are split into those that 'define' a route versus those that are
            // 'silent'.
            //
            // We need to define an action for each attribute that 'defines' a route, and a single action
            // for all of the ones that don't (if any exist).
            //
            // If the attribute that 'defines' a route is NOT an IActionHttpMethodProvider, then we'll include with
            // it, any IActionHttpMethodProvider that are 'silent' IRouteTemplateProviders. In this case the 'extra'
            // action for silent route providers isn't needed.
            //
            // Ex:
            // [HttpGet]
            // [AcceptVerbs("POST", "PUT")]
            // [HttpPost("Api/Things")]
            // public void DoThing()
            //
            // This will generate 2 actions:
            // 1. [HttpPost("Api/Things")]
            // 2. [HttpGet], [AcceptVerbs("POST", "PUT")]
            //
            // Note that having a route attribute that doesn't define a route template _might_ be an error. We
            // don't have enough context to really know at this point so we just pass it on.
            var routeProviders = new List<object>();

            var createActionForSilentRouteProviders = false;
            foreach (var attribute in attributes)
            {
                var routeTemplateProvider = attribute as IRouteTemplateProvider;
                if (routeTemplateProvider != null)
                {
                    if (IsSilentRouteAttribute(routeTemplateProvider))
                    {
                        createActionForSilentRouteProviders = true;
                    }
                    else
                    {
                        routeProviders.Add(attribute);
                    }
                }
            }

            foreach (var routeProvider in routeProviders)
            {
                // If we see an attribute like
                // [Route(...)]
                //
                // Then we want to group any attributes like [HttpGet] with it.
                //
                // Basically...
                //
                // [HttpGet]
                // [HttpPost("Products")]
                // public void Foo() { }
                //
                // Is two actions. And...
                //
                // [HttpGet]
                // [Route("Products")]
                // public void Foo() { }
                //
                // Is one action.
                if (!(routeProvider is IActionHttpMethodProvider))
                {
                    createActionForSilentRouteProviders = false;
                }
            }

            var actionModels = new List<ActionModel>();
            if (routeProviders.Count == 0 && !createActionForSilentRouteProviders)
            {
                actionModels.Add(CreateActionModel(methodInfo, attributes));
            }
            else
            {
                // Each of these routeProviders are the ones that actually have routing information on them
                // something like [HttpGet] won't show up here, but [HttpGet("Products")] will.
                foreach (var routeProvider in routeProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (attribute == routeProvider)
                        {
                            filteredAttributes.Add(attribute);
                        }
                        else if (routeProviders.Contains(attribute))
                        {
                            // Exclude other route template providers
                        }
                        else if (
                            routeProvider is IActionHttpMethodProvider &&
                            attribute is IActionHttpMethodProvider)
                        {
                            // Exclude other http method providers if this route is an
                            // http method provider.
                        }
                        else
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    actionModels.Add(CreateActionModel(methodInfo, filteredAttributes));
                }

                if (createActionForSilentRouteProviders)
                {
                    var filteredAttributes = new List<object>();
                    foreach (var attribute in attributes)
                    {
                        if (!routeProviders.Contains(attribute))
                        {
                            filteredAttributes.Add(attribute);
                        }
                    }

                    actionModels.Add(CreateActionModel(methodInfo, filteredAttributes));
                }
            }

            return actionModels;
        }

        /// <summary>
        /// Returns <c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="typeInfo">The <see cref="TypeInfo"/>.</param>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="methodInfo"/> is an action. Otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Override this method to provide custom logic to determine which methods are considered actions.
        /// </remarks>
        protected virtual bool IsAction(TypeInfo typeInfo, MethodInfo methodInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(NonActionAttribute)))
            {
                return false;
            }

            // Overriden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
            {
                return false;
            }

            // Dispose method implemented from IDisposable is not valid
            if (IsIDisposableMethod(methodInfo, typeInfo))
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                return false;
            }

            if (methodInfo.IsAbstract)
            {
                return false;
            }

            if (methodInfo.IsConstructor)
            {
                return false;
            }

            if (methodInfo.IsGenericMethod)
            {
                return false;
            }

            return methodInfo.IsPublic;
        }

        /// <summary>
        /// Creates an <see cref="ActionModel"/> for the given <see cref="MethodInfo"/>.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
        /// <param name="attributes">The set of attributes to use as metadata.</param>
        /// <returns>An <see cref="ActionModel"/> for the given <see cref="MethodInfo"/>.</returns>
        /// <remarks>
        /// An action-method in code may expand into multiple <see cref="ActionModel"/> instances depending on how
        /// the action is routed. In the case of multiple routing attributes, this method will invoked be once for
        /// each action that can be created.
        ///
        /// If overriding this method, use the provided <paramref name="attributes"/> list to find metadata related to
        /// the action being created.
        /// </remarks>
        protected virtual ActionModel CreateActionModel(
            MethodInfo methodInfo,
            IReadOnlyList<object> attributes)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var actionModel = new ActionModel(methodInfo, attributes);

            AddRange(actionModel.ActionConstraints, attributes.OfType<IActionConstraintMetadata>());
            AddRange(actionModel.Filters, attributes.OfType<IFilterMetadata>());

            var actionName = attributes.OfType<ActionNameAttribute>().FirstOrDefault();
            if (actionName?.Name != null)
            {
                actionModel.ActionName = actionName.Name;
            }
            else
            {
                actionModel.ActionName = methodInfo.Name;
            }

            var apiVisibility = attributes.OfType<IApiDescriptionVisibilityProvider>().FirstOrDefault();
            if (apiVisibility != null)
            {
                actionModel.ApiExplorer.IsVisible = !apiVisibility.IgnoreApi;
            }

            var apiGroupName = attributes.OfType<IApiDescriptionGroupNameProvider>().FirstOrDefault();
            if (apiGroupName != null)
            {
                actionModel.ApiExplorer.GroupName = apiGroupName.GroupName;
            }

            var httpMethods = attributes.OfType<IActionHttpMethodProvider>();
            AddRange(actionModel.HttpMethods,
                httpMethods
                    .Where(a => a.HttpMethods != null)
                    .SelectMany(a => a.HttpMethods)
                    .Distinct());

            AddRange(actionModel.RouteConstraints, attributes.OfType<IRouteConstraintProvider>());

            var routeTemplateProvider =
                attributes
                .OfType<IRouteTemplateProvider>()
                .SingleOrDefault(a => !IsSilentRouteAttribute(a));

            if (routeTemplateProvider != null)
            {
                actionModel.AttributeRouteModel = new AttributeRouteModel(routeTemplateProvider);
            }

            return actionModel;
        }

        /// <summary>
        /// Creates a <see cref="ParameterModel"/> for the given <see cref="ParameterInfo"/>.
        /// </summary>
        /// <param name="parameterInfo">The <see cref="ParameterInfo"/>.</param>
        /// <returns>A <see cref="ParameterModel"/> for the given <see cref="ParameterInfo"/>.</returns>
        protected virtual ParameterModel CreateParameterModel(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }

            // CoreCLR returns IEnumerable<Attribute> from GetCustomAttributes - the OfType<object>
            // is needed to so that the result of ToArray() is object
            var attributes = parameterInfo.GetCustomAttributes(inherit: true).OfType<object>().ToArray();
            var parameterModel = new ParameterModel(parameterInfo, attributes);

            var bindingInfo = BindingInfo.GetBindingInfo(attributes);
            parameterModel.BindingInfo = bindingInfo;

            parameterModel.ParameterName = parameterInfo.Name;

            return parameterModel;
        }

        private bool IsIDisposableMethod(MethodInfo methodInfo, TypeInfo typeInfo)
        {
            return
                (typeof(IDisposable).GetTypeInfo().IsAssignableFrom(typeInfo) &&
                 typeInfo.GetRuntimeInterfaceMap(typeof(IDisposable)).TargetMethods[0] == methodInfo);
        }

        private bool IsSilentRouteAttribute(IRouteTemplateProvider routeTemplateProvider)
        {
            return
                routeTemplateProvider.Template == null &&
                routeTemplateProvider.Order == null &&
                routeTemplateProvider.Name == null;
        }

        private static void AddRange<T>(IList<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}