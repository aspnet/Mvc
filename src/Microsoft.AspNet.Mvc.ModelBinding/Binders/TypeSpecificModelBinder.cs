// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{

    public class TypeSpecificModelBinder : IModelBinder
    {
        public TypeSpecificModelBinder(Type modelType, IModelBinder modelBinder)
        {
            this.modelType = modelType;
            this.modelBinder = modelBinder;
        }

        public Type modelType
        {
            get;
            private set;
        }

        public IModelBinder modelBinder
        {
            get;
            private set;
        }

        public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != modelType)
            {
                return false;
            }
            return await modelBinder.BindModelAsync(bindingContext);
        }
    }
}
