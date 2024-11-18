using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Services;
using MyStore.Application.Services.Users;
using MyStore.Domain.Enumerations;
using System.Security.Claims;

namespace MyStore.Presentation.Controllers
{
    [Route("api/account")]
    [ApiController]
    [Authorize]
    public class AccountsController(IUserService userService, IAuthenticationService authenticationService) : ControllerBase
    {
        private readonly IUserService _userService = userService;
        private readonly IAuthenticationService _authenticationService = authenticationService;

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PageRequest request, [FromQuery] RolesEnum role)
        {
            try
            {
                var users = await _userService.GetAllUsersAsync(request.Page, request.PageSize, request.Key, role);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("address")]
        public async Task<IActionResult> GetAddress()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var address = await _userService.GetUserAddress(userId);
                return Ok(address);
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

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] AccountRequest request)
        {
            try
            {
                var user = await _authenticationService.CreateUserWithRoles(request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAccount(string userId, [FromBody] UpdateAccountRequest request)
        {
            try
            {
                var user = await _authenticationService.UpdateUserAccount(userId, request);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPut("address")]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressDTO address)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var result = await _userService.UpdateOrCreateUserAddress(userId, address);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetInfo()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var address = await _userService.GetUserInfo(userId);
                return Ok(address);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("info")]
        public async Task<IActionResult> UpdateInfo([FromBody] UserInfo request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var result = await _userService.UpdateUserInfo(userId, request);
                return Ok(result);
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

        [HttpGet("favorite")]
        public async Task<IActionResult> GetFavorite()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var favorites = await _userService.GetFavorites(userId);
                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("favorite")]
        public async Task<IActionResult> AddFavorite([FromBody] IdRequest<long> request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _userService.AddProductFavorite(userId, request.Id);
                return Created();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("favorite/products")]
        public async Task<IActionResult> ProductFavorites([FromQuery] PageRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var pFavorites = await _userService.GetProductFavorites(userId, request);
                return Ok(pFavorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("favorite/{productId}")]
        public async Task<IActionResult> DeleteFavorite(long productId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _userService.DeleteProductFavorite(userId, productId);
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

        [HttpPut("lock-out/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LockOut(string id, [FromBody] LockOutRequest request)
        {
            try
            {
                await _userService.LockOut(id, request.EndDate);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
