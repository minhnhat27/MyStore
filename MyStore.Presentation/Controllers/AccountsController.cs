using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services;

namespace MyStore.Presentation.Controllers
{
    [Route("api/account")]
    [ApiController]
    [AllowAnonymous]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.Login(request);
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status202Accepted, content);
            else return StatusCode((int)result.StatusCode, content);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        [ProducesResponseType(StatusCodes.Status411LengthRequired)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status201Created);
            else return StatusCode((int) result.StatusCode, content);
        }

        [HttpPost("sendCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendCode([FromBody] StringRequest request)
        {
            var result  = await _accountService.SendCode(request);
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
                return Ok();
            else return StatusCode((int) result.StatusCode, content);
        }
    }
}
