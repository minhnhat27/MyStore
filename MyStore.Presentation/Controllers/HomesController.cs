using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.IStorage;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;
using MyStore.Domain.Constants;

namespace MyStore.Presentation.Controllers
{
    [Route("api/home")]
    [ApiController]
    public class HomesController(IProductService productService, 
        IWebHostEnvironment environment, IFileStorage fileStorage) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IFileStorage _fileStorage = fileStorage;

        [HttpGet("banner")]
        public IActionResult GetBanner()
        {
            try
            {
                var path = "assets/images/banner";
                var bannerPath = Path.Combine(_environment.WebRootPath, path);
                List<string> filePaths = new();
                if (Directory.Exists(bannerPath))
                {
                    var files = Directory.GetFiles(bannerPath);
                    filePaths.AddRange(files.Select(file => Path.Combine(path, Path.GetFileName(file))));
                }
                return Ok(filePaths);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("banner")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner([FromForm] IEnumerable<string>? imageUrls, [FromForm] IFormFileCollection? images)
        {
            try
            {
                var path = "assets/images/banner";
                var bannerPath = Path.Combine(_environment.WebRootPath, path);

                var listDelete = new List<string>();

                if (Directory.Exists(bannerPath))
                {
                    var files = Directory.GetFiles(bannerPath);
                    var filePaths = files.Select(file => Path.Combine(path, Path.GetFileName(file)));
                    if (imageUrls == null)
                    {
                        if (filePaths.Any())
                        {
                            listDelete.AddRange(filePaths);
                        }
                    }
                    else
                    {
                        listDelete.AddRange(filePaths.Where(e => !imageUrls.Contains(e)));
                    }
                }

                if (listDelete.Any())
                {
                    _fileStorage.Delete(listDelete);
                }

                if(images != null)
                {
                    var fileNames = images.Select(e => Guid.NewGuid().ToString() + Path.GetExtension(e.FileName)).ToList();
                    await _fileStorage.SaveAsync(bannerPath, images, fileNames);
                }

                return NoContent();
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
                var result = await _productService
                    .GetGetFeaturedProductsAsync(request.Page, request.PageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
