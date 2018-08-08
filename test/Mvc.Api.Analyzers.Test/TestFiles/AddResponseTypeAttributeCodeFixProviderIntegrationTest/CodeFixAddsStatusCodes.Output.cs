namespace Microsoft.AspNetCore.Mvc.Api.Analyzers._OUTPUT_
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class CodeFixAddsStatusCodesController : ControllerBase
    {
        [ProducesResponseType(Http.StatusCodes.Status200OK)]
        [ProducesResponseType(Http.StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public IActionResult GetItem(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            return Ok(new object());
        }
    }
}
