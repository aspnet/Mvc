// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class FromQueryModelBinder : MarkerAwareBinder<FromQueryAttribute>
    {
        public override async Task<bool> BindAsync(ModelBindingContext bindingContext)
        {
            // now filter out the value providers. 
            var newBindingContext = new ModelBindingContext(bindingContext);
            newBindingContext.ValueProvider = GetValueProvider(bindingContext.OriginalValueProvider);
            newBindingContext.ModelMetadata = bindingContext.ModelMetadata;
            newBindingContext.ModelName = bindingContext.ModelName;

            // Null out the marker so that this model binder does not get selected again.
            // TODO: Come up with a better way of acheiving this.
            newBindingContext.ModelMetadata.Marker = null;
            return await newBindingContext.ModelBinder.BindModelAsync(newBindingContext);
        }

        public IValueProvider GetValueProvider(IValueProvider valueProvider)
        {
            if (valueProvider is ReadableStringCollectionValueProvider)
            {
                return valueProvider;
            }

            IValueProvider filteredVP = null;
            var compositeValueProvider = valueProvider as CompositeValueProvider;
            if(compositeValueProvider != null)
            {
                // there is no distinction at the value provider level. Query value provider is same as
                filteredVP = compositeValueProvider.FirstOrDefault(vp => vp.GetType() == typeof(ReadableStringCollectionValueProvider));
            }

            return filteredVP;
        }
    }
}
