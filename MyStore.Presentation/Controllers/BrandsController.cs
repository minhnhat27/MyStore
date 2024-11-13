using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Brands;

namespace MyStore.Presentation.Controllers
{
    [Route("api/brands")]
    [ApiController]
    public class BrandsController(IBrandService brandService) : ControllerBase
    {
        private readonly IBrandService _brandService = brandService;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                return Ok(await _brandService.GetBrandsAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularBrands()
        {
            try
            {
                return Ok(await _brandService.GetTop5PopularBrandsAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.First();
                var brand = await _brandService.AddBrandAsync(request.Name, image);
                return Ok(brand);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] NameRequest request, [FromForm] IFormCollection files)
        {
            try
            {
                var image = files.Files.FirstOrDefault();
                var brand = await _brandService.UpdateBrandAsync(id, request.Name, image);
                return Ok(brand);
            }
            catch(ArgumentException ex)
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
                await _brandService.DeleteBrandAsync(id);
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
