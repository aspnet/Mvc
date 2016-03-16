// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Internal
{
    public class DelegatingBinder : IModelBinder
    {
        public bool IsInUse { get; set; }

        public IModelBinder Inner { get; set; }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            return Inner.BindModelAsync(bindingContext);
        }
    }
}
