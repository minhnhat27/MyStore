using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services;

namespace MyStore.Presentation.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _accountService.Register(request);
            if (result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status201Created);
            else return Conflict(result);
        }
    }
}
