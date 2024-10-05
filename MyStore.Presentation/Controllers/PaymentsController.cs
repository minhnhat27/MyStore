using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.DTOs;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Services.Payments;

namespace MyStore.Presentation.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        [HttpGet]
        public async Task<IActionResult> GetPaymentMethods()
        {
            try
            {
                var result = await _paymentService.GetPaymentMethods();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] CreatePaymentMethodRequest request)
        {
            try
            {
                var result = await _paymentService.CreatePaymentMethod(request);
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePaymentMethodRequest request)
        {
            try
            {
                var result = await _paymentService.UpdatePaymentMethod(id, request);
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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _paymentService.DeletePaymentMethod(id);
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


        [HttpGet("vnpay-callback")]
        public async Task<IActionResult> VNPayCallback([FromQuery] VNPayRequest request)
        {
            try
            {
                await _paymentService.VNPayCallback(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("payos-callback")]
        public async Task<IActionResult> PayOSCallback([FromQuery] PayOSRequest request)
        {
            try
            {
                await _paymentService.PayOSCallback(request);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
