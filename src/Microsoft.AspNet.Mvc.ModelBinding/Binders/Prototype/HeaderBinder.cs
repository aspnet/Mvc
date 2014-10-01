// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public class HeaderBinder : MarkerAwareBinder<FromHeaderAttribute>
    {
        public override Task<bool> BindAsync(ModelBindingContext bindingContext)
        {
            // TODO: Add the piece which looks at type converter.
            if(bindingContext.HttpContext.Request.Headers.TryGetValue(Marker.HeaderKey, out var headerValue))
            {
                // TODO: if this is a collection or it supports typeconverter, do that here. 
                bindingContext.Model = headerValue.First();
            }

            return Task.FromResult<bool>(true);
        }
    }
}
