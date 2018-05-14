// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    public interface IRazorCompilationCache
    {
        bool TryGetCompiledView(string relativePath, out Task<CompiledViewDescriptor> result);
        void SetCompiledView(string relativePath, Task<CompiledViewDescriptor> compiledViewDescriptor, MemoryCacheEntryOptions options);
    }
}
