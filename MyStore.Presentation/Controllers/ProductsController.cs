﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;

namespace MyStore.Presentation.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

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
                Console.WriteLine(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("create")]
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

        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromForm] ProductRequest request, [FromForm] IFormFileCollection images)
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
        public async Task<IActionResult> UpdateProductEnable(int id, [FromBody] UpdateEnableRequest request)
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

        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
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
    }
}
