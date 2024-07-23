using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Payments;

namespace MyStore.Presentation.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentsController(IPaymentService paymentService) : ControllerBase
    {
        private readonly IPaymentService _paymentService = paymentService;

        [HttpGet("payment-method-is-active")]
        public async Task<IActionResult> GetPaymentMethodsIsActive()
        {
            return Ok(await _paymentService.GetPaymentMethodsIsActive());
        }

        [HttpGet("payment-methods")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPaymentMethods()
        {
            return Ok(await _paymentService.GetPaymentMethods());
        }
    }
}
