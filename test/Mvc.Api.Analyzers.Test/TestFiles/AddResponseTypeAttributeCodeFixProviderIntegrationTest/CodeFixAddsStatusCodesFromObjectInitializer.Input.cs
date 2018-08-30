﻿using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers._INPUT_
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CodeFixAddsStatusCodesFromObjectInitializerController : ControllerBase
    {
        private const int FieldStatusCode = 201;

        public IActionResult GetItem(int id)
        {
            if (id == 0)
            {
                return new ObjectResult(new object())
                {
                    StatusCode = 422
                };
            }

            if (id == 1)
            {
                return new ObjectResult(new object())
                {
                    StatusCode = StatusCodes.Status202Accepted
                };
            }

            if (id == 2)
            {
                const int localStatusCode = 204;

                return new ObjectResult(new object())
                {
                    StatusCode = localStatusCode
                };
            }

            if (id == 3)
            {
                return new ObjectResult(new object())
                {
                    StatusCode = FieldStatusCode
                };
            }

            return Ok(new object());
        }
    }
}
