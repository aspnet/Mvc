// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding
{
    /// <summary>
    /// <see cref="IModelBinder"/> implementation to bind models of type <see cref="CancellationToken"/>.
    /// </summary>
    public class CancellationTokenModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.ModelType == typeof(CancellationToken))
            {
                // We need to force boxing now, so we can insert the same reference to the boxed CancellationToken
                // in both the ValidationState and ModelBindingResult.
                //
                // DO NOT simplify this code by removing the cast.
                var model = (object)bindingContext.OperationBindingContext.HttpContext.RequestAborted;
                bindingContext.Result = ModelBindingResult.Success(bindingContext.ModelName, model);
            }

            return TaskCache.CompletedTask;
        }
    }
}
