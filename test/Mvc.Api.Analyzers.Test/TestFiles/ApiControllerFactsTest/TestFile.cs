﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers
{
    public class ApiConventionAnalyzerTest_IndexModel : PageModel
    {
        public IActionResult OnGet() => null;
    }

    public class ApiConventionAnalyzerTest_NotApiController : Controller
    {
        public IActionResult Index() => null;
    }

    public class ApiConventionAnalyzerTest_NotAction : Controller
    {
        [NonAction]
        public IActionResult Index() => null;
    }

    [ApiController]
    public class ApiConventionAnalyzerTest_Valid : Controller
    {
        public IActionResult Index() => null;
    }
}
