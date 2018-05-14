// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// Caches the compiled view in memory.
    /// </summary>
    internal class DefaultRazorCompilationCache : IRazorCompilationCache
    {
        private readonly IMemoryCache _cache;

        public DefaultRazorCompilationCache()
        {
            // This is our L0 cache, and is a durable store. Views migrate into the cache as they are requested
            // from either the set of known precompiled views, or by being compiled.
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        public void SetCompiledView(string relativePath, Task<CompiledViewDescriptor> compiledViewDescriptor, MemoryCacheEntryOptions options)
        {
            _cache.Set(relativePath, compiledViewDescriptor, options);
        }

        public bool TryGetCompiledView(string relativePath, out Task<CompiledViewDescriptor> result)
        {
            return _cache.TryGetValue(relativePath, out result);
        }
    }
}
