// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents a model binder which can bind models of type <see cref="CancellationToken"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="CancellationToken"/> provided by this binder is invoked after a supplied timeout value.
    /// This binder depends on the <see cref="ITimeoutCancellationTokenFeature"/> to get the token. This feature
    /// is typically set through the <see cref="AsyncTimeoutAttribute"/>.
    /// </remarks>
    public class TimeoutCancellationTokenModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public Task<ModelBindingResult> BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(CancellationToken))
            {
                var httpContext = bindingContext.OperationBindingContext.HttpContext;

                var timeoutTokenFeature = httpContext.GetFeature<ITimeoutCancellationTokenFeature>();
                if (timeoutTokenFeature != null)
                {
                    return Task.FromResult(new ModelBindingResult(
                        model: timeoutTokenFeature.TimeoutCancellationToken,
                        key: bindingContext.ModelName, 
                        isModelSet: true));
                }
                else
                {
                    return Task.FromResult(new ModelBindingResult(
                        model: CancellationToken.None,
                        key: bindingContext.ModelName,
                        isModelSet: false));
                }
            }

            return Task.FromResult<ModelBindingResult>(null);
        }
    }
}
