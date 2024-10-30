using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;
using System;

namespace MyStore.Presentation.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController(IProductService productService, IWebHostEnvironment environment) : ControllerBase
    {
        private readonly IProductService _productService = productService;
        private readonly IWebHostEnvironment _environment = environment;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _productService.GetProductsAsync(request.Page, request.PageSize, request.Key);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            try
            {
                var result = await _productService.GetProductAsync(id);
                return Ok(result);
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

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterProducts([FromQuery] ProductFiltersRequest filters)
        {
            try
            {
                var result = await _productService.GetFilterProductsAsync(filters);
                return Ok(result);
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

        [HttpGet("search")]
        public async Task<IActionResult> GetSearchProducts([FromQuery] string key)
        {
            try
            {
                var result = await _productService.GetSearchProducts(key);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("search/image")]
        public async Task<IActionResult> GetSearchProducts([FromForm] IFormFile image)
        {
            try
            {
                var rootPath = Path.Combine(_environment.WebRootPath);

                var tempFilePath = Path.GetTempFileName();
                using(var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                var result = await _productService.GetSearchProducts(tempFilePath, rootPath);
                System.IO.File.Delete(tempFilePath);

                return Ok(result);
            }
            catch(FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] ProductRequest request, [FromForm] IFormFileCollection images)
        {
            try
            {
                var product = await _productService.CreateProductAsync(request, images);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(long id, [FromForm] ProductRequest request, [FromForm] IFormFileCollection images)
        {
            try
            {
                var result = await _productService.UpdateProductAsync(id, request, images);
                return Ok(result);
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

        [HttpPut("updateEnable/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductEnable(long id, [FromBody] UpdateEnableRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductEnableAsync(id, request);
                return Ok(result);
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
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
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

        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetReviews(long id, [FromQuery] ReviewFiltersRequest request)
        {
            try
            {
                var result = await _productService.GetReviews(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
