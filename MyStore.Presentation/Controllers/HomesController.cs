using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Products;
using MyStore.Domain.Constants;

namespace MyStore.Presentation.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomesController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _environment;
        public HomesController(IProductService productService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

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
    }
}
