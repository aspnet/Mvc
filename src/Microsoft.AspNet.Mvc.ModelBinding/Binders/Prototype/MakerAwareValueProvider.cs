// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Mvc.ModelBinding
{
    public abstract class MarkerAwareValueProvider<T> : IValueProvider
        where T : IValueBinderMarker
    {
        public bool IsValidFor(Type valueProviderType)
        {
            return typeof(T).IsAssignableFrom(valueProviderType);
        }

        public abstract Task<bool> ContainsPrefixAsync(string prefix);

        public abstract Task<ValueProviderResult> GetValueAsync(string key);
    }
}
