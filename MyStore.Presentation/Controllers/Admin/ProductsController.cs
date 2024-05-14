using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Admin.Request;
using MyStore.Application.Request;
using MyStore.Application.Services.Products;

namespace MyStore.Presentation.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductsController(IProductService productService) => _productService = productService;

        [HttpGet("getProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts()
        {
            return Ok(await _productService.GetProductsAsync());
        }

        [HttpGet("getProduct/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
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
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("createProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateProductEnable")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteProduct/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("getBrands")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBrands()
        {
            return Ok(await _productService.GetBrandsAsync());
        }
        [HttpGet("getBrandNames")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBrandNames()
        {
            return Ok(await _productService.GetBrandNamesAsync());
        }


        [HttpPost("addBrand")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddBrand([FromForm] CreateBrandRequest request)
        {
            try
            {
                await _productService.AddBrandAsync(request);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteBrand/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                var result = await _productService.DeleteBrandAsync(id);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //--//
        [HttpGet("getCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            return Ok(await _productService.GetCategoriesAsync());
        }

        [HttpPost("addCategory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddCategory([FromBody] NameRequest request)
        {
            try
            {
                await _productService.AddCategoryAsync(request);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpDelete("deleteCategory/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(id);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //--//
        [HttpGet("getMaterials")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMaterials()
        {
            return Ok(await _productService.GetMaterialsAsync());
        }

        [HttpPost("addMaterial")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddMaterial([FromBody] NameRequest request)
        {
            try
            {
                await _productService.AddMaterialAsync(request);
                return Created();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteMaterial/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            try
            {
                var result = await _productService.DeleteMaterialAsync(id);
                if (result)
                    return Ok();
                else return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //--//
        [HttpGet("getSizes")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSizes()
        {
            return Ok(await _productService.GetSizesAsync());
        }

        [HttpPost("addSize")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddSize([FromBody] CreateSizeRequest request)
        {
            try
            {
                await _productService.AddSizeAsync(request);
                return Created();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("deleteSize/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSize(int id)
        {
            try
            {
                var result = await _productService.DeleteSizeAsync(id);
                if (result)
                {
                    return Ok();
                }
                else return NotFound();
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
