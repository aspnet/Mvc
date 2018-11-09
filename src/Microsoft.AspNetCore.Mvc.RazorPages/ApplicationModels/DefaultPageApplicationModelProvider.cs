﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Resources = Microsoft.AspNetCore.Mvc.RazorPages.Resources;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    internal class DefaultPageApplicationModelProvider : IPageApplicationModelProvider
    {
        private const string ModelPropertyName = "Model";
        private readonly PageHandlerPageFilter _pageHandlerPageFilter = new PageHandlerPageFilter();
        private readonly PageHandlerResultFilter _pageHandlerResultFilter = new PageHandlerResultFilter();
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly MvcOptions _mvcOptions;
        private readonly RazorPagesOptions _razorPagesOptions;
        private readonly Func<ActionContext, bool> _supportsAllRequests;
        private readonly Func<ActionContext, bool> _supportsNonGetRequests;
        private readonly HandleOptionsRequestsPageFilter _handleOptionsRequestsFilter;

        public DefaultPageApplicationModelProvider(
            IModelMetadataProvider modelMetadataProvider,
            IOptions<MvcOptions> options,
            IOptions<RazorPagesOptions> razorPagesOptions)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _mvcOptions = options.Value;
            _razorPagesOptions = razorPagesOptions.Value;

            _supportsAllRequests = _ => true;
            _supportsNonGetRequests = context => !HttpMethods.IsGet(context.HttpContext.Request.Method);
            _handleOptionsRequestsFilter = new HandleOptionsRequestsPageFilter();
        }

        /// <inheritdoc />
        public int Order => -1000;

        /// <inheritdoc />
        public void OnProvidersExecuting(PageApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.PageApplicationModel = CreateModel(context.ActionDescriptor, context.PageType);
        }

        /// <inheritdoc />
        public void OnProvidersExecuted(PageApplicationModelProviderContext context)
        {
        }

        /// <summary>
        /// Creates a <see cref="PageApplicationModel"/> for the given <paramref name="pageTypeInfo"/>.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="PageActionDescriptor"/>.</param>
        /// <param name="pageTypeInfo">The <see cref="TypeInfo"/>.</param>
        /// <returns>A <see cref="PageApplicationModel"/> for the given <see cref="TypeInfo"/>.</returns>
        protected virtual PageApplicationModel CreateModel(
            PageActionDescriptor actionDescriptor,
            TypeInfo pageTypeInfo)
        {
            if (actionDescriptor == null)
            {
                throw new ArgumentNullException(nameof(actionDescriptor));
            }

            if (pageTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(pageTypeInfo));
            }

            if (!typeof(PageBase).GetTypeInfo().IsAssignableFrom(pageTypeInfo))
            {
                throw new InvalidOperationException(Resources.FormatInvalidPageType_WrongBase(
                    pageTypeInfo.FullName,
                    typeof(PageBase).FullName));
            }

            // Pages always have a model type. If it's not set explicitly by the developer using
            // @model, it will be the same as the page type.
            var modelProperty = pageTypeInfo.GetProperty(ModelPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (modelProperty == null)
            {
                throw new InvalidOperationException(Resources.FormatInvalidPageType_NoModelProperty(
                    pageTypeInfo.FullName,
                    ModelPropertyName));
            }

            var modelTypeInfo = modelProperty.PropertyType.GetTypeInfo();
            var declaredModelType = modelTypeInfo;

            // Now we want figure out which type is the handler type.
            TypeInfo handlerType;
            if (modelProperty.PropertyType.IsDefined(typeof(PageModelAttribute), inherit: true))
            {
                handlerType = modelTypeInfo;
            }
            else
            {
                handlerType = pageTypeInfo;
            }

            var handlerTypeAttributes = handlerType.GetCustomAttributes(inherit: true);
            var pageModel = new PageApplicationModel(
                actionDescriptor,
                declaredModelType,
                handlerType,
                handlerTypeAttributes)
            {
                PageType = pageTypeInfo,
                ModelType = modelTypeInfo,
            };

            PopulateHandlerMethods(pageModel);
            PopulateHandlerProperties(pageModel);
            PopulateFilters(pageModel);

            return pageModel;
        }

        // Internal for unit testing
        internal void PopulateHandlerProperties(PageApplicationModel pageModel)
        {
            var properties = PropertyHelper.GetVisibleProperties(pageModel.HandlerType.AsType());

            for (var i = 0; i < properties.Length; i++)
            {
                var propertyModel = CreatePropertyModel(properties[i].Property);
                if (propertyModel != null)
                {
                    propertyModel.Page = pageModel;
                    pageModel.HandlerProperties.Add(propertyModel);
                }
            }
        }

        // Internal for unit testing
        internal void PopulateHandlerMethods(PageApplicationModel pageModel)
        {
            var methods = pageModel.HandlerType.GetMethods();

            for (var i = 0; i < methods.Length; i++)
            {
                var handler = CreateHandlerModel(methods[i]);
                if (handler != null)
                {
                    pageModel.HandlerMethods.Add(handler);
                }
            }
        }

        internal void PopulateFilters(PageApplicationModel pageModel)
        {
            for (var i = 0; i < pageModel.HandlerTypeAttributes.Count; i++)
            {
                if (pageModel.HandlerTypeAttributes[i] is IFilterMetadata filter)
                {
                    pageModel.Filters.Add(filter);
                }
            }

            if (typeof(IAsyncPageFilter).IsAssignableFrom(pageModel.HandlerType) ||
                typeof(IPageFilter).IsAssignableFrom(pageModel.HandlerType))
            {
                pageModel.Filters.Add(_pageHandlerPageFilter);
            }

            if (typeof(IAsyncResultFilter).IsAssignableFrom(pageModel.HandlerType) ||
                typeof(IResultFilter).IsAssignableFrom(pageModel.HandlerType))
            {
                pageModel.Filters.Add(_pageHandlerResultFilter);
            }

            if (_razorPagesOptions.AllowDefaultHandlingForOptionsRequests)
            {
                pageModel.Filters.Add(_handleOptionsRequestsFilter);
            }
        }

        /// <summary>
        /// Creates a <see cref="PageHandlerModel"/> for the specified <paramref name="method"/>.s
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo"/>.</param>
        /// <returns>The <see cref="PageHandlerModel"/>.</returns>
        protected virtual PageHandlerModel CreateHandlerModel(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (!IsHandler(method))
            {
                return null;
            }

            if (!TryParseHandlerMethod(method.Name, out var httpMethod, out var handlerName))
            {
                return null;
            }

            var handlerModel = new PageHandlerModel(
                method,
                method.GetCustomAttributes(inherit: true))
            {
                Name = method.Name,
                HandlerName = handlerName,
                HttpMethod = httpMethod,
            };

            var methodParameters = handlerModel.MethodInfo.GetParameters();

            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];
                var parameterModel = CreateParameterModel(parameter);
                parameterModel.Handler = handlerModel;

                handlerModel.Parameters.Add(parameterModel);
            }

            return handlerModel;
        }

        /// <summary>
        /// Creates a <see cref="PageParameterModel"/> for the specified <paramref name="parameter"/>.
        /// </summary>
        /// <param name="parameter">The <see cref="ParameterInfo"/>.</param>
        /// <returns>The <see cref="PageParameterModel"/>.</returns>
        protected virtual PageParameterModel CreateParameterModel(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var attributes = parameter.GetCustomAttributes(inherit: true);

            BindingInfo bindingInfo;
            if (_mvcOptions.AllowValidatingTopLevelNodes && _modelMetadataProvider is ModelMetadataProvider modelMetadataProviderBase)
            {
                var modelMetadata = modelMetadataProviderBase.GetMetadataForParameter(parameter);
                bindingInfo = BindingInfo.GetBindingInfo(attributes, modelMetadata);
            }
            else
            {
                bindingInfo = BindingInfo.GetBindingInfo(attributes);
            }

            return new PageParameterModel(parameter, attributes)
            {
                BindingInfo = bindingInfo,
                ParameterName = parameter.Name,
            };
        }

        /// <summary>
        /// Creates a <see cref="PagePropertyModel"/> for the <paramref name="property"/>.
        /// </summary>
        /// <param name="property">The <see cref="PropertyInfo"/>.</param>
        /// <returns>The <see cref="PagePropertyModel"/>.</returns>
        protected virtual PagePropertyModel CreatePropertyModel(PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            var propertyAttributes = property.GetCustomAttributes(inherit: true);

            // BindingInfo for properties can be either specified by decorating the property with binding-specific attributes.
            // ModelMetadata also adds information from the property's type and any configured IBindingMetadataProvider.
            var propertyMetadata = _modelMetadataProvider.GetMetadataForProperty(property.DeclaringType, property.Name);
            var bindingInfo = BindingInfo.GetBindingInfo(propertyAttributes, propertyMetadata);

            if (bindingInfo == null)
            {
                // Look for BindPropertiesAttribute on the handler type if no BindingInfo was inferred for the property.
                // This allows a user to enable model binding on properties by decorating the controller type with BindPropertiesAttribute.
                var declaringType = property.DeclaringType;
                var bindPropertiesAttribute = declaringType.GetCustomAttribute<BindPropertiesAttribute>(inherit: true);
                if (bindPropertiesAttribute != null)
                {
                    var requestPredicate = bindPropertiesAttribute.SupportsGet ? _supportsAllRequests : _supportsNonGetRequests;
                    bindingInfo = new BindingInfo
                    {
                        RequestPredicate = requestPredicate,
                    };
                }
            }

            var model = new PagePropertyModel(property, propertyAttributes)
            {
                PropertyName = property.Name,
                BindingInfo = bindingInfo,
            };

            return model;
        }

        /// <summary>
        /// Determines if the specified <paramref name="methodInfo"/> is a handler.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
        /// <returns><c>true</c> if the <paramref name="methodInfo"/> is a handler. Otherwise <c>false</c>.</returns>
        /// <remarks>
        /// Override this method to provide custom logic to determine which methods are considered handlers.
        /// </remarks>
        protected virtual bool IsHandler(MethodInfo methodInfo)
        {
            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            // Overridden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
            if (methodInfo.GetBaseDefinition().DeclaringType == typeof(object))
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

            if (!methodInfo.IsPublic)
            {
                return false;
            }

            if (methodInfo.IsDefined(typeof(NonHandlerAttribute)))
            {
                return false;
            }

            // Exclude the whole hierarchy of Page.
            var declaringType = methodInfo.DeclaringType;
            if (declaringType == typeof(Page) ||
                declaringType == typeof(PageBase) ||
                declaringType == typeof(RazorPageBase))
            {
                return false;
            }

            // Exclude methods declared on PageModel
            if (declaringType == typeof(PageModel))
            {
                return false;
            }

            return true;
        }

        internal static bool TryParseHandlerMethod(string methodName, out string httpMethod, out string handler)
        {
            httpMethod = null;
            handler = null;

            // Handler method names always start with "On"
            if (!methodName.StartsWith("On") || methodName.Length <= "On".Length)
            {
                return false;
            }

            // Now we parse the method name according to our conventions to determine the required HTTP method
            // and optional 'handler name'.
            //
            // Valid names look like:
            //  - OnGet
            //  - OnPost
            //  - OnFooBar
            //  - OnTraceAsync
            //  - OnPostEditAsync

            var start = "On".Length;
            var length = methodName.Length;
            if (methodName.EndsWith("Async", StringComparison.Ordinal))
            {
                length -= "Async".Length;
            }

            if (start == length)
            {
                // There are no additional characters. This is "On" or "OnAsync".
                return false;
            }

            // The http method follows "On" and is required to be at least one character. We use casing
            // to determine where it ends.
            var handlerNameStart = start + 1;
            for (; handlerNameStart < length; handlerNameStart++)
            {
                if (char.IsUpper(methodName[handlerNameStart]))
                {
                    break;
                }
            }

            httpMethod = methodName.Substring(start, handlerNameStart - start);

            // The handler name follows the http method and is optional. It includes everything up to the end
            // excluding the "Async" suffix (if present).
            handler = handlerNameStart == length ? null : methodName.Substring(handlerNameStart, length - handlerNameStart);
            return true;
        }
    }
}
