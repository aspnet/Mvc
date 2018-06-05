﻿using Microsoft.AspNetCore.Authorization;

namespace Microsoft.AspNetCore.Mvc.Analyzers.Test
{
    public class NoDiagnosticsAreReturned_ForControllerBaseActions : ControllerBase
    {
        [Authorize]
        public IActionResult AuthorizeAttribute() => null;

        [ServiceFilter(typeof(object))]
        public IActionResult ServiceFilter() => null;
    }
}
