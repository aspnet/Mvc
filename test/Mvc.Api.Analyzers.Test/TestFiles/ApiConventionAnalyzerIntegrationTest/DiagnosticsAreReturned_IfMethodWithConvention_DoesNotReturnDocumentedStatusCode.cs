﻿using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers
{
    [ApiController]
    public class DiagnosticsAreReturned_IfMethodWithConvention_DoesNotReturnDocumentedStatusCode : ControllerBase
    {
        public IActionResult /*MM*/Delete(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            return Ok();
        }
    }
}
