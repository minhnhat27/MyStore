using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Admin.Request;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;

namespace MyStore.Presentation.Controllers
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService) => _productService = productService;

        [HttpGet("getProducts")]
        public async Task<IActionResult> GetProducts([FromQuery] PageRequest request)
        {
            return Ok(await _productService.GetProductsAsync(request.Page, request.PageSize, request.Key));
        }

        [HttpGet("getProduct/{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var result = await _productService.GetProductAsync(id);
                if (result != null)
                {
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("createProduct")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductRequest request, [FromForm] IFormCollection form)
        {
            try
            {
                var images = form.Files;
                await _productService.CreateProductAsync(request, images);
                return Created();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("updateProduct")]
        public async Task<IActionResult> UpdateProduct([FromForm] UpdateProductRequest request, [FromForm] IFormCollection form)
        {
            try
            {
                var images = form.Files;
                var result = await _productService.UpdateProductAsync(request, images);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPut("updateProductEnable")]
        public async Task<IActionResult> UpdateProductEnable(UpdateProductEnableRequest request)
        {
            try
            {
                var result = await _productService.UpdateProductEnableAsync(request);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("deleteProduct/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpGet("getBrands")]
        public async Task<IActionResult> GetBrands()
        {
            return Ok(await _productService.GetBrandsAsync());
        }

        [HttpPost("addBrand")]
        public async Task<IActionResult> AddBrand([FromForm] CreateBrandRequest request)
        {
            try
            {
                await _productService.AddBrandAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("deleteBrand/{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var result = await _productService.DeleteBrandAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //--//
        [HttpGet("getCategories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                return Ok(await _productService.GetCategoriesAsync());
            }
            catch(Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("addCategory")]
        public async Task<IActionResult> AddCategory([FromBody] NameRequest request)
        {
            try
            {
                await _productService.AddCategoryAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("deleteCategory/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //--//
        [HttpGet("getMaterials")]
        public async Task<IActionResult> GetMaterials()
        {
            try
            {
                return Ok(await _productService.GetMaterialsAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("addMaterial")]
        public async Task<IActionResult> AddMaterial([FromBody] NameRequest request)
        {
            try
            {
                await _productService.AddMaterialAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("deleteMaterial/{id}")]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                var result = await _productService.DeleteMaterialAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        //--//
        [HttpGet("getSizes")]
        public async Task<IActionResult> GetSizes()
        {
            try
            {
                return Ok(await _productService.GetSizesAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpPost("addSize")]
        public async Task<IActionResult> AddSize([FromBody] CreateSizeRequest request)
        {
            try
            {
                await _productService.AddSizeAsync(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        [HttpDelete("deleteSize/{id}")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            try
            {
                var result = await _productService.DeleteSizeAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
    }
}
