﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Microsoft.AspNetCore.Mvc.Analyzers.Test
{
    [PageModel]
    public class DiagnosticsAreReturned_IfFiltersAreAppliedToPageHandlerMethodForTypeWithPageModelAttribute : PageModel
    {
        [/*MM*/ServiceFilter(typeof(object))]
        public void OnGet()
        {
        }
    }
}
