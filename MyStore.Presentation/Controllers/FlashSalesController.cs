using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.FlashSales;
using MyStore.Domain.Enumerations;

namespace MyStore.Presentation.Controllers
{
    [Route("api/flashsales")]
    [ApiController]
    public class FlashSalesController(IFlashSaleService flashSaleService) : ControllerBase
    {
        public readonly IFlashSaleService _flashSaleService = flashSaleService;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get([FromQuery] PageRequest request)
        {
            try
            {
                var result = await _flashSaleService.GetFlashSales(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("products/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductsByFlashSale(string id)
        {
            try
            {
                var result = await _flashSaleService.GetProductsByFlashSale(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("time")]
        public async Task<IActionResult> GetProductsThisTime(int timeFrame)
        {
            try
            {
                var result = await _flashSaleService.GetFlashSaleThisTime();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("time/{timeFrame}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetProductsByTimeFrame(DiscountTimeFrame timeFrame)
        {
            try
            {
                var result = await _flashSaleService.GetProductsByTimeFrame(timeFrame);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateFlashSale([FromBody] FlashSaleRequest request)
        {
            try
            {
                var result = await _flashSaleService.CreateFlashSale(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id, [FromBody] FlashSaleRequest request)
        {
            try
            {
                var result = await _flashSaleService.UpdateFlashSale(id, request);
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
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _flashSaleService.DeleteFlashSale(id);
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
