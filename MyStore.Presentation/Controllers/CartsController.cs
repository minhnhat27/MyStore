using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Services.Carts;
using System.Security.Claims;

namespace MyStore.Presentation.Controllers
{
    [Route("api/cart")]
    [ApiController]
    [Authorize]
    public class CartsController(ICartService cartService) : ControllerBase
    {
        private readonly ICartService _cartService = cartService;

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var cartItems = await _cartService.GetAllByUserId(userId);
                return Ok(cartItems);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
