﻿using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers._INPUT_
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CodeFixAddsMissingStatusCodesAndTypes : ControllerBase
    {
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetItem(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (id == 1)
            {
                return BadRequest(ModelState);
            }

            return Ok(new object());
        }
    }
}
