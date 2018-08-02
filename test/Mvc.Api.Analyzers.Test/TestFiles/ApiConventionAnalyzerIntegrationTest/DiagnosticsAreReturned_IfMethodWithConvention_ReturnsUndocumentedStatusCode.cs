﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Api.Analyzers;

[assembly: ApiConventionType(typeof(DiagnosticsAreReturned_IfMethodWithConvention_ReturnsUndocumentedStatusCodeConvention))]

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers
{
    [ApiController]
    public class DiagnosticsAreReturned_IfMethodWithConvention_ReturnsUndocumentedStatusCode : ControllerBase
    {
        public IActionResult Get(int id)
        {
            if (id < 0)
            {
                /*MM*/return BadRequest();
            }

            if (id == 0)
            {
                return NotFound();
            }

            return Ok();
        }
    }

    public static class DiagnosticsAreReturned_IfMethodWithConvention_ReturnsUndocumentedStatusCodeConvention
    {
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public static void Get(int id) { }
    }
}
