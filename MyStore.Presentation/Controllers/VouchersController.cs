using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Vouchers;
using System.Security.Claims;

namespace MyStore.Presentation.Controllers
{
    [Route("api/vouchers")]
    [ApiController]
    [Authorize]
    public class VouchersController(IVoucherService voucherService) : ControllerBase
    {
        private readonly IVoucherService _voucherService = voucherService;

        [HttpGet]
        public async Task<IActionResult> GetVoucherByUser()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if(userId == null)
                {
                    return Unauthorized();
                }
                var vouchers = await _voucherService.GetVoucherByUser(userId);
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
