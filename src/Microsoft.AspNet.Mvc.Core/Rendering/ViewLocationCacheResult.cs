// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Mvc.Rendering
{
    /// <summary>
    /// Result of a <see cref="IViewLocationCache.Get(System.Collections.Generic.SortedDictionary{string, string})."/>
    /// </summary>
    public class ViewLocationCacheResult
    {
        public ViewLocationCacheResult([NotNull] string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }

        public string Value { get; private set; }
    }
}