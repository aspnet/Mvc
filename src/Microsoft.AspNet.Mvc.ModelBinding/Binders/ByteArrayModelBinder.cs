﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc.ModelBinding.Internal;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// ModelBinder to bind Byte Arrays.
    /// </summary>
    public class ByteArrayModelBinder : IModelBinder
    {
        /// <inheritdoc />
        public async Task<bool> BindModelAsync([NotNull] ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType != typeof(byte[]))
            {
                return false;
            }

            var valueProviderResult = await bindingContext.ValueProviders.GetValueAsync(bindingContext.ModelName);

            // case 1: there was no <input ... /> element containing this data
            if (valueProviderResult == null)
            {
                return false;
            }

            var value = valueProviderResult.AttemptedValue;

            // case 2: there was an <input ... /> element but it was left blank
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            try
            {
                bindingContext.Model = Convert.FromBase64String(value);
            }
            catch (Exception ex)
            {
                ModelBindingHelper.AddModelErrorBasedOnExceptionType(bindingContext, ex);
            }
            
            return true;
        }
    }
}