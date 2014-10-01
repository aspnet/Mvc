// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc
{
    public class ReflectedActionInvokerProvider : IActionInvokerProvider
    {
        private readonly IControllerFactory _controllerFactory;
        private readonly IActionBindingContextProvider _bindingProvider;
        private readonly IInputFormattersProvider _inputFormattersProvider;
        private readonly INestedProviderManager<FilterProviderContext> _filterProvider;
        private readonly IActionInvocationInfoBinder _actionInvocationBinder;

        public ReflectedActionInvokerProvider(IControllerFactory controllerFactory,
                                              IActionBindingContextProvider bindingProvider,
                                              IInputFormattersProvider inputFormattersProvider,
                                              INestedProviderManager<FilterProviderContext> filterProvider,
                                              [NotNull] IActionInvocationInfoBinder actionInvocationBinder)
        {
            _controllerFactory = controllerFactory;
            _bindingProvider = bindingProvider;
            _inputFormattersProvider = inputFormattersProvider;
            _filterProvider = filterProvider;
            _actionInvocationBinder = actionInvocationBinder;
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
                                    _bindingProvider,
                                    _filterProvider,
                                    _controllerFactory,
                                    actionDescriptor,
                                    _inputFormattersProvider,
                                    _actionInvocationBinder);
            }

            callNext();
        }
    }
}
