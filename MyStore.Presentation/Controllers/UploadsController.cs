using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MyStore.Presentation.Controllers
{
    [Route("api/upload")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        [HttpPost("product")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Product([FromForm] IFormCollection files)
        {
            return Ok();
        }
    }
}
