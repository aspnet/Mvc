﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    public class DefaultPageLoader : IPageLoader
    {
        private const string ModelPropertyName = "Model";

        private readonly IViewCompilerProvider _viewCompilerProvider;

        public DefaultPageLoader(IViewCompilerProvider viewCompilerProvider)
        {
            _viewCompilerProvider = viewCompilerProvider;
        }

        private IViewCompiler Compiler => _viewCompilerProvider.GetCompiler();

        public CompiledPageActionDescriptor Load(PageActionDescriptor actionDescriptor)
        {
            var compileTask = Compiler.CompileAsync(actionDescriptor.RelativePath);
            var viewDescriptor = compileTask.GetAwaiter().GetResult();
            var pageAttribute = (RazorPageAttribute)viewDescriptor.ViewAttribute;

            return CreateDescriptor(actionDescriptor, pageAttribute);
        }

        // Internal for unit testing
        internal static CompiledPageActionDescriptor CreateDescriptor(
            PageActionDescriptor actionDescriptor,
            RazorPageAttribute pageAttribute)
        {
            var pageType = pageAttribute.ViewType.GetTypeInfo();
            if (!typeof(PageBase).IsAssignableFrom(pageType))
            {
                throw new InvalidOperationException(Resources.FormatInvalidPageType_WrongBase(
                    pageType.FullName,
                    typeof(PageBase).FullName));
            }

            // Pages always have a model type.If it's not set explicitly by the developer using
            // @model, it will be the same as the page type.
            var modelProperty = pageType.GetProperty(ModelPropertyName);
            if (modelProperty == null)
            {
                throw new InvalidOperationException(Resources.FormatInvalidPageType_NoModelProperty(
                    pageType.FullName,
                    ModelPropertyName));
            }

            var modelType = modelProperty.PropertyType.GetTypeInfo();

            // Now we want figure out which type is the handler type.
            TypeInfo handlerType;
            if (pageType == modelType)
            {
                handlerType = pageType;
            }
            else if (modelType.IsDefined(typeof(PageModelAttribute), inherit: true))
            {
                handlerType = modelType;
            }
            else
            {
                handlerType = pageType;
            }

            var handlerMethods = CreateHandlerMethods(handlerType);
            var boundProperties = CreateBoundProperties(handlerType);

            return new CompiledPageActionDescriptor(actionDescriptor)
            {
                ActionConstraints = actionDescriptor.ActionConstraints,
                AttributeRouteInfo = actionDescriptor.AttributeRouteInfo,
                BoundProperties = boundProperties,
                FilterDescriptors = actionDescriptor.FilterDescriptors,
                HandlerMethods = handlerMethods,
                HandlerTypeInfo = handlerType,
                ModelTypeInfo = modelType,
                RouteValues = actionDescriptor.RouteValues,
                PageTypeInfo = pageType,
                Properties = actionDescriptor.Properties,
            };
        }

        internal static HandlerMethodDescriptor[] CreateHandlerMethods(TypeInfo type)
        {
            var methods = type.GetMethods();
            var results = new List<HandlerMethodDescriptor>();

            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                if (!IsValidHandlerMethod(method))
                {
                    continue;
                }

                if (method.IsDefined(typeof(NonHandlerAttribute)))
                {
                    continue;
                }

                // Exclude the whole hierarchy of Page.
                if (method.DeclaringType == typeof(Page) ||
                    method.DeclaringType == typeof(PageBase) ||
                    method.DeclaringType == typeof(RazorPageBase))
                {
                    continue;
                }

                // Exclude everything on PageModel
                if (method.DeclaringType == typeof(PageModel))
                {
                    continue;
                }

                if (!TryParseHandlerMethod(method.Name, out var httpMethod, out var handler))
                {
                    continue;
                }

                var parameters = CreateHandlerParameters(method);

                var handlerMethodDescriptor = new HandlerMethodDescriptor()
                {
                    MethodInfo = method,
                    Name = handler,
                    HttpMethod = httpMethod,
                    Parameters = parameters,
                };

                results.Add(handlerMethodDescriptor);
            }

            return results.ToArray();
        }

        // Internal for testing
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

        private static bool IsValidHandlerMethod(MethodInfo methodInfo)
        {
            // The SpecialName bit is set to flag members that are treated in a special way by some compilers
            // (such as property accessors and operator overloading methods).
            if (methodInfo.IsSpecialName)
            {
                return false;
            }

            // Overriden methods from Object class, e.g. Equals(Object), GetHashCode(), etc., are not valid.
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

            return methodInfo.IsPublic;
        }

        // Internal for testing
        internal static HandlerParameterDescriptor[] CreateHandlerParameters(MethodInfo methodInfo)
        {
            var methodParameters = methodInfo.GetParameters();
            var parameters = new HandlerParameterDescriptor[methodParameters.Length];

            for (var i = 0; i < methodParameters.Length; i++)
            {
                var parameter = methodParameters[i];

                parameters[i] = new HandlerParameterDescriptor()
                {
                    BindingInfo = BindingInfo.GetBindingInfo(parameter.GetCustomAttributes()),
                    Name = parameter.Name,
                    ParameterInfo = parameter,
                    ParameterType = parameter.ParameterType,
                };
            }

            return parameters;
        }

        // Internal for testing
        internal static PageBoundPropertyDescriptor[] CreateBoundProperties(TypeInfo type)
        {
            var properties = PropertyHelper.GetVisibleProperties(type.AsType());

            var results = new List<PageBoundPropertyDescriptor>();
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var bindingInfo = BindingInfo.GetBindingInfo(property.Property.GetCustomAttributes());

                // If there's no binding info then that means there are no model binding attributes on the
                // property. So we won't bind this property.
                if (bindingInfo == null)
                {
                    continue;
                }

                var descriptor = new PageBoundPropertyDescriptor()
                {
                    BindingInfo = bindingInfo,
                    Name = property.Name,
                    Property = property.Property,
                    ParameterType = property.Property.PropertyType,
                };

                results.Add(descriptor);
            }

            return results.ToArray();
        }
    }
}