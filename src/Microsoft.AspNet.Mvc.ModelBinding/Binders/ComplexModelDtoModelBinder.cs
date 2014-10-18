// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public sealed class ComplexModelDtoModelBinder : IModelBinder
    {
        public async Task<bool> BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(ComplexModelDto))
            {
                ModelBindingHelper.ValidateBindingContext(bindingContext,
                                                          typeof(ComplexModelDto),
                                                          allowNullModel: false);

                var dto = (ComplexModelDto)bindingContext.Model;
                
                foreach (var propertyMetadata in dto.PropertyMetadata)
                {
                    var propertyBindingContext = new ModelBindingContext(bindingContext)
                    {
                        ModelMetadata = propertyMetadata,
                        ModelName = ModelBindingHelper.CreatePropertyModelName(bindingContext.ModelName,
                                                                               propertyMetadata.PropertyName), 
                    };

                    // bind and propagate the values
                    // If we can't bind, then leave the result missing (don't add a null).
                    if (await bindingContext.ModelBinder.BindModelAsync(propertyBindingContext))
                    {
                        var result = new ComplexModelDtoResult(propertyBindingContext.Model,
                                                               propertyBindingContext.ValidationNode);
                        dto.Results[propertyMetadata] = result;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
