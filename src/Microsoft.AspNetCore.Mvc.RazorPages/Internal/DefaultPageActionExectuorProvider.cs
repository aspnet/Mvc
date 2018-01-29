using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace Microsoft.AspNetCore.Mvc.RazorPages.Internal
{
    class DefaultPageActionExectuorProvider : IPageActionExectuorProvider
    {
        public Func<object, object[], Task<IActionResult>>[] GetExecutors(CompiledPageActionDescriptor actionDescriptor)
        {

            if(actionDescriptor.HandlerMethods == null || actionDescriptor.HandlerMethods.Count == 0)
            {
                return Array.Empty<Func<object, object[], Task<IActionResult>>>();
            }

            var results = new Func<object, object[], Task<IActionResult>>[actionDescriptor.HandlerMethods.Count];

            for(var i = 0; i < actionDescriptor.HandlerMethods.Count; i++)
            {
                results[i] = ExecutorFactory.CreateExecutor(actionDescriptor.HandlerMethods[i]);
            }

            return results;
        }
    }
}
