using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.DTOs;
using MyStore.Application.Request;
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

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var vouchers = await _voucherService.GetAllVoucher();
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] VoucherDTO request)
        {
            try
            {
                var vouchers = await _voucherService.CreateVoucher(request);
                return Ok(vouchers);
            }
            catch (InvalidDataException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("{code}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string code)
        {
            try
            {
                await _voucherService.DeleteVoucher(code);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{code}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateGlobalVoucher(string code, [FromBody] UpdateEnableRequest request)
        {
            try
            {
                var value = await _voucherService.UpdateIsGlobal(code, request.Enable);
                return Ok(value);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("user/{code}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserVoucher(string code)
        {
            try
            {
                var users = await _voucherService.GetUserVoucher(code);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("user/{code}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUserVoucher(string code, [FromBody] UserHaveVoucher request)
        {
            try
            {
                var users = await _voucherService.UpdateUserHaveVoucher(code, request.HaveVoucher);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

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

        [HttpPost("add-voucher")]
        public async Task<IActionResult> ApplyVoucher([FromBody] CodeRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var voucher = await _voucherService.GetCommonVoucher(userId, request.Code);
                return Ok(voucher);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
