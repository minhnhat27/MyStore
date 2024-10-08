using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;
using MyStore.Domain.Constants;

namespace MyStore.Presentation.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomesController(IProductService productService, IWebHostEnvironment environment) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        private readonly IWebHostEnvironment _environment = environment;

        [HttpGet("banner")]
        public IActionResult GetBanner()
        {
            try
            {
                var path = "assets/images/banner";
                var bannerPath = Path.Combine(_environment.WebRootPath, path);
                if (!Directory.Exists(bannerPath))
                {
                    return NotFound(ErrorMessage.FOLDER_NOT_FOUND);
                }
                var files = Directory.GetFiles(bannerPath);
                var filePaths = files.Select(file => Path.Combine(path, Path.GetFileName(file)));

                return Ok(filePaths);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedProducts([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _productService.GetGetFeaturedProductsAsync(request.Page, request.PageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
