namespace Microsoft.AspNetCore.Mvc.Api.Analyzers._OUTPUT_
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CodeFixAddsMissingStatusCodes : ControllerBase
    {
        [ProducesResponseType(404)]
        [ProducesResponseType(Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Http.StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult GetItem(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            if (id == 1)
            {
                return BadRequest();
            }

            return Ok(new object());
        }
    }
}
