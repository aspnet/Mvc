using Microsoft.AspNetCore.Mvc;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]

namespace Microsoft.AspNetCore.Mvc.Api.Analyzers._OUTPUT_
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CodeFixAddsSuccessStatusCode : ControllerBase
    {
        [ProducesResponseType(Http.StatusCodes.Status201Created)]
        [ProducesResponseType(Http.StatusCodes.Status400BadRequest)]
        [ProducesResponseType(Http.StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<object> GetItem(string id)
        {
            if (!int.TryParse(id, out var idInt))
            {
                return BadRequest();
            }

            if (idInt == 0)
            {
                return NotFound();
            }

            return Created("url", new object());
        }
    }
}
