using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.AspNetCore.Mvc.Razor.Compilation
{
    /// <summary>
    /// Provides an instance of <see cref="IMemoryCache"/> that is used to stop compiled Razor views. 
    /// </summary>
    public interface IViewCompilationMemoryCacheProvider
    {
        IMemoryCache CompilationMemoryCache { get; }
    }
}
