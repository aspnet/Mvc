// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// An <see cref="IModelBinder"/> which binds models from the request services when a model 
    /// has the binding source <see cref="BindingSource.Services"/>/
    /// </summary>
    public class ServicesModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            var allowedBindingSource = bindingContext.BindingSource;
            if (allowedBindingSource == null ||
                !allowedBindingSource.CanAcceptDataFrom(BindingSource.Services))
            {
                // Services are opt-in. This model either didn't specify [FromService] or specified something
                // incompatible so let other binders run.
                return ModelBindingResult.NoResultAsync;
            }

            var requestServices = bindingContext.OperationBindingContext.HttpContext.RequestServices;
            var model = requestServices.GetRequiredService(bindingContext.ModelType);
            var validationNode =
                new ModelValidationNode(bindingContext.ModelName, bindingContext.ModelMetadata, model)
                {
                    SuppressValidation = true
                };

            return ModelBindingResult.SuccessAsync(bindingContext.ModelName, model, validationNode);
        }
    }
}
