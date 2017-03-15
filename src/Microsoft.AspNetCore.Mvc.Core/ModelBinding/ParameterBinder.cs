// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// Binds and validates models specified by a <see cref="ParameterDescriptor"/>.
    /// </summary>
    public class ParameterBinder
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ParameterDescriptor"/>.
        /// </summary>
        /// <param name="modelMetadataProvider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="modelBinderFactory">The <see cref="IModelBinderFactory"/>.</param>
        /// <param name="validator">The <see cref="IObjectModelValidator"/>.</param>
        public ParameterBinder(
            IModelMetadataProvider modelMetadataProvider,
            IModelBinderFactory modelBinderFactory,
            IObjectModelValidator validator)
        {
            if (modelMetadataProvider == null)
            {
                throw new ArgumentNullException(nameof(modelMetadataProvider));
            }

            if (modelBinderFactory == null)
            {
                throw new ArgumentNullException(nameof(modelBinderFactory));
            }

            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            ModelMetadataProvider = modelMetadataProvider;
            ModelBinderFactory = modelBinderFactory;
            Validator = validator;
        }

        /// <summary>
        /// The <see cref="IModelMetadataProvider"/>.
        /// </summary>
        public IModelMetadataProvider ModelMetadataProvider { get; }

        /// <summary>
        /// The <see cref="IModelBinderFactory"/>.
        /// </summary>
        public IModelBinderFactory ModelBinderFactory { get; }

        /// <summary>
        /// The <see cref="IObjectModelValidator"/>.
        /// </summary>
        public IObjectModelValidator Validator { get; }

        /// <summary>
        /// Initializes and binds a model specified by <paramref name="parameter"/>.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/></param>
        /// <returns>The result of model binding.</returns>
        public Task<ModelBindingResult> BindModelAsync(
            ActionContext actionContext,
            IValueProvider valueProvider,
            ParameterDescriptor parameter)
        {
            return BindModelAsync(actionContext, valueProvider, parameter, value: null);
        }

        /// <summary>
        /// Binds a model specified by <paramref name="parameter"/> using <paramref name="value"/> as the initial value.
        /// </summary>
        /// <param name="actionContext">The <see cref="ActionContext"/>.</param>
        /// <param name="valueProvider">The <see cref="IValueProvider"/>.</param>
        /// <param name="parameter">The <see cref="ParameterDescriptor"/></param>
        /// <param name="value">The initial model value.</param>
        /// <returns>The result of model binding.</returns>
        public virtual async Task<ModelBindingResult> BindModelAsync(
            ActionContext actionContext,
            IValueProvider valueProvider,
            ParameterDescriptor parameter,
            object value)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (valueProvider == null)
            {
                throw new ArgumentNullException(nameof(valueProvider));
            }

            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            var metadata = ModelMetadataProvider.GetMetadataForType(parameter.ParameterType);
            var binder = ModelBinderFactory.CreateBinder(new ModelBinderFactoryContext()
            {
                BindingInfo = parameter.BindingInfo,
                Metadata = metadata,
                CacheToken = parameter,
            });

            var modelBindingContext = DefaultModelBindingContext.CreateBindingContext(
                actionContext,
                valueProvider,
                metadata,
                parameter.BindingInfo,
                parameter.Name);
            modelBindingContext.Model = value;

            var parameterModelName = parameter.BindingInfo?.BinderModelName ?? metadata.BinderModelName;
            if (parameterModelName != null)
            {
                // The name was set explicitly, always use that as the prefix.
                modelBindingContext.ModelName = parameterModelName;
            }
            else if (modelBindingContext.ValueProvider.ContainsPrefix(parameter.Name))
            {
                // We have a match for the parameter name, use that as that prefix.
                modelBindingContext.ModelName = parameter.Name;
            }
            else
            {
                // No match, fallback to empty string as the prefix.
                modelBindingContext.ModelName = string.Empty;
            }

            await binder.BindModelAsync(modelBindingContext);

            var modelBindingResult = modelBindingContext.Result;
            if (modelBindingResult.IsModelSet)
            {
                Validator.Validate(
                    actionContext,
                    modelBindingContext.ValidationState,
                    modelBindingContext.ModelName,
                    modelBindingResult.Model);
            }

            return modelBindingResult;
        }
    }
}
