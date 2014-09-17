// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class MarkerAwareBinder<T> : IModelBinder
        where T : IBinderMarker
    {
        public abstract Task<bool> BindAsync(ModelBindingContext bindingContext);

        public Task<bool> BindModelAsync(ModelBindingContext context)
        {
            if(typeof(T) == context.ModelMetadata.Marker?.GetType())
            {
                Marker = (T)context.ModelMetadata.Marker;
                return BindAsync(context);
            }

            return Task.FromResult<bool>(false);
        }

        public T Marker { get; set; }
    }
}
