// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding;

namespace Microsoft.AspNet.Mvc
{
    public class DefaultActionBindingContextProvider : IActionBindingContextProvider
    {
        private readonly IModelMetadataProvider _modelMetadataProvider;
        private readonly ICompositeModelBinder _compositeModelBinder;
        private readonly IValueProviderFactoryProvider _valueProviderFactoryProvider;
        private readonly IInputFormatterSelector _inputFormatterSelector;
        private readonly ICompositeModelValidatorProvider _validatorProvider;
        private Tuple<ActionContext, ActionBindingContext> _bindingContext;

        public DefaultActionBindingContextProvider(IModelMetadataProvider modelMetadataProvider,
                                                   ICompositeModelBinder compositeModelBinder,
                                                   IValueProviderFactoryProvider valueProviderFactoryProvider,
                                                   IInputFormatterSelector inputFormatterProvider,
                                                   ICompositeModelValidatorProvider validatorProvider)
        {
            _modelMetadataProvider = modelMetadataProvider;
            _compositeModelBinder = compositeModelBinder;
            _valueProviderFactoryProvider = valueProviderFactoryProvider;
            _inputFormatterSelector = inputFormatterProvider;
            _validatorProvider = validatorProvider;
        }

        public Task<ActionBindingContext> GetActionBindingContextAsync(ActionContext actionContext)
        {
            if (_bindingContext != null)
            {
                if (actionContext == _bindingContext.Item1)
                {
                    return Task.FromResult(_bindingContext.Item2);
                }
            }

            var factoryContext = new ValueProviderFactoryContext(
                                    actionContext.HttpContext,
                                    actionContext.RouteData.Values);

            var valueProviders = _valueProviderFactoryProvider.ValueProviderFactories
                                                              .Select(factory => factory.GetValueProvider(factoryContext))
                                                              .Where(vp => vp != null);

            var context = new ActionBindingContext(
                actionContext,
                _modelMetadataProvider,
                _compositeModelBinder,
                valueProviders.ToList(),
                _inputFormatterSelector,
                _validatorProvider);

            _bindingContext = new Tuple<ActionContext, ActionBindingContext>(actionContext, context);

            return Task.FromResult(context);
        }
    }
}
