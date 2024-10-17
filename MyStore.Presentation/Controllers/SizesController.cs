using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Sizes;

namespace MyStore.Presentation.Controllers
{
    [Route("api/sizes")]
    [ApiController]
    public class SizesController(ISizeService sizeService) : ControllerBase
    {
        private readonly ISizeService _sizeService = sizeService;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _sizeService.GetSizesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] NameRequest request)
        {
            try
            {
                var size = await _sizeService.AddSizeAsync(request.Name);
                return Ok(size);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] NameRequest request)
        {
            try
            {
                var size = await _sizeService.UpdateSizeAsync(id, request.Name);
                return Ok(size);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _sizeService.DeleteSizeAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
