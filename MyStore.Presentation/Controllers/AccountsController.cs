using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Accounts;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _accountService.Login(request);
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
                return Ok(content);
            else return StatusCode((int)result.StatusCode);
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status411LengthRequired)]
        [ProducesResponseType(StatusCodes.Status417ExpectationFailed)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            return StatusCode((int)result.StatusCode);
        }

        [HttpPost("sendCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> SendCode([FromBody] StringRequest request)
        {
            var result  = await _accountService.SendCode(request);
            return StatusCode((int)result.StatusCode);
        }


        [HttpPost("loginGoogle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoginGoogle([FromBody] StringRequest request)
        {
            var result = await _accountService.LoginGoogle(request);
            var content = await result.Content.ReadAsStringAsync();

            if (result.IsSuccessStatusCode)
                return Ok(content);
            else return StatusCode((int)result.StatusCode);
        }
    }
}
