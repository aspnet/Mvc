using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc.RazorPages
{
    /// <summary>
    /// Provides methods for get or creation page executors of Razor pages.
    /// </summary>
    public interface IPageActionExectuorProvider
    {
        /// <summary>
        /// Get executors of <paramref name="actionDescriptor"/>.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="CompiledPageActionDescriptor"/>.</param>
        /// <returns>The page action executors.</returns>
        Func<object, object[], Task<IActionResult>>[] GetExecutors(CompiledPageActionDescriptor actionDescriptor);
    }
}
