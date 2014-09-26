// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    /// <summary>
    /// Represents a marker used to identify a particular binder applies to an artifact.
    /// </summary>
    public static class ValueProviderCollectionExtensions
    {
        public static async Task<bool> ContainsPrefixAsync(this IEnumerable<IValueProvider> valueProviders,
                                                           string prefix)
        {
            foreach (var valueProvider in valueProviders)
            {
                if (await valueProvider.ContainsPrefixAsync(prefix))
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task<ValueProviderResult> GetValueAsync(this IEnumerable<IValueProvider> valueProviders,
                                                                    string key)
        {
            foreach (var valueProvider in valueProviders)
            {
                var result = await valueProvider.GetValueAsync(key);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
