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
            // [harshg] this could even be a value coming from value provider. Why not?
            // The reason would be to not have other binders kick in for this + this does depend on the fact that
            // there is a header key and not the path ( on which value providers work on). 
            // TODO: Add the piece which looks at type converter.
            if(bindingContext.HttpContext.Request.Headers.TryGetValue(Marker.HeaderKey, out var headerValue))
            {
                // TODO: if this is a collection or it supports typeconverter, do that here. 
                bindingContext.Model = headerValue.First();
                return Task.FromResult<bool>(true);
            }

            return Task.FromResult<bool>(false);
        }
    }
}
