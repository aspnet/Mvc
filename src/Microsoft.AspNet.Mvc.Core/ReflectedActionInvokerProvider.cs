// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionInvokerProvider : IActionInvokerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ITypeActivator _typeActivator;
        private readonly IControllerActivator _controllerActivator;
        private readonly IActionBindingContextProvider _bindingProvider;
        private readonly INestedProviderManager<FilterProviderContext> _filterProvider;

        public ReflectedActionInvokerProvider(IControllerActivator controllerActivator,
                                              IActionBindingContextProvider bindingProvider,
                                              INestedProviderManager<FilterProviderContext> filterProvider,
                                              IServiceProvider serviceProvider,
                                              ITypeActivator typeActivator)
        {
            _controllerActivator = controllerActivator;
            _bindingProvider = bindingProvider;
            _filterProvider = filterProvider;
            _serviceProvider = serviceProvider;
            _typeActivator = typeActivator;
        }

        public int Order
        {
            get { return 0; }
        }

        public void Invoke(ActionInvokerProviderContext context, Action callNext)
        {
            var actionDescriptor = context.ActionContext.ActionDescriptor as ReflectedActionDescriptor;

            if (actionDescriptor != null)
            {
                context.Result = new ReflectedActionInvoker(
                                    context.ActionContext,
                                    actionDescriptor,
                                    _serviceProvider,
                                    _typeActivator,
                                    _controllerActivator,
                                    _bindingProvider,
                                    _filterProvider);
            }

            callNext();
        }
    }
}
