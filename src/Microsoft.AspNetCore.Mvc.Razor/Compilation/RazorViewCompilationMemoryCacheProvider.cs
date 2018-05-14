using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// And implementation of <see cref="IViewCompilationMemoryCacheProvider"/> using a local <see cref="IMemoryCache"/> instance.
    /// </summary>
    public class RazorViewCompilationMemoryCacheProvider : IViewCompilationMemoryCacheProvider
    {
        IMemoryCache IViewCompilationMemoryCacheProvider.CompilationMemoryCache { get; } = new MemoryCache(new MemoryCacheOptions());
    }
}
