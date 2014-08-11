// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// ModelBinder to bind Byte Arrays.
    /// </summary>
    public class ByteArrayModelBinder : IModelBinder
    {
        /// <summary>
        /// Async function to bind Byte Arrays.
        /// </summary>
        /// <param name="bindingContext">The binding context which has the object to be bound.</param>
        /// <returns>A Task with a bool implying the success or failure of the operation.</returns>
        public async Task<bool> BindModelAsync([NotNull] ModelBindingContext bindingContext)
        {
            var valueProviderResult = await bindingContext.ValueProvider.GetValueAsync(bindingContext.ModelName);

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

            // Future proofing. If the byte array is actually an instance of System.Data.Linq.Binary
            // then we need to remove these quotes put in place by the ToString() method.
            bindingContext.Model = Convert.FromBase64String(value.Replace("\"", string.Empty));
            return true;
        }
    }
}