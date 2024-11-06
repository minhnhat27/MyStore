using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Orders;
using MyStore.Domain.Enumerations;
using System.Security.Claims;

namespace MyStore.Presentation.Controllers
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;

        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest request)
        {
            try
            {
                return Ok(await _orderService.GetAll(request.Page, request.PageSize, request.Key));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrderWithStatus(DeliveryStatusEnum status, [FromQuery] PageRequest request)
        {
            try
            {
                var result = await _orderService.GetWithOrderStatus(status, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("next-status/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AcceptOrder(long orderId, [FromBody] AcceptOrderRequest request)
        {
            try
            {
                await _orderService.NextOrderStatus(orderId, request.CurrentStatus);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpPut("shipping/{orderId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Shipping(long orderId, [FromBody] OrderToShippingRequest request)
        {
            try
            {
                await _orderService.OrderToShipping(orderId, request);
                return Ok();
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
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

        [HttpGet]
        public async Task<IActionResult> Orders([FromQuery] PageRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var orders = await _orderService.GetOrdersByUserId(userId, request);
                return Ok(orders);
            }
            catch(ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> OrderDetails(long id)
        {
            try
            {
                var roles = User.FindAll(ClaimTypes.Role).Select(e => e.Value);
                var isAdmin = roles.Contains("Admin");

                if(isAdmin)
                {
                    var orders = await _orderService.GetOrderDetail(id);
                    return Ok(orders);
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized();
                    }
                    var orders = await _orderService.GetOrderDetail(id, userId);
                    return Ok(orders);
                }
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var orders = await _orderService.CreateOrder(userId, request);
                return Ok(orders);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                var orders = await _orderService.UpdateOrder(id, userId, request);
                return Ok(orders);
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
        public async Task<IActionResult> Cancel(long id)
        {
            try
            {
                var roles = User.FindAll(ClaimTypes.Role).Select(e => e.Value);
                var isAdmin = roles.Contains("Admin");

                if (isAdmin)
                {
                    await _orderService.CancelOrder(id);
                    return Ok();
                }
                else
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId == null)
                    {
                        return Unauthorized();
                    }
                    await _orderService.CancelOrder(id, userId);
                    return Ok();
                }
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(ex.Message);
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


        [HttpPost("review/{id}")]
        public async Task<IActionResult> Review(long id, [FromForm] IEnumerable<ReviewRequest> reviews)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _orderService.Review(id, userId, reviews);
                return Ok();
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
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }


        [HttpPut("confirm-delivery/{id}")]
        public async Task<IActionResult> ConfirmDelivery(long id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized();
                }
                await _orderService.ConfirmDelivery(id, userId);
                return Ok();
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
                return StatusCode(500, ex.InnerException?.Message ?? ex.Message);
            }
        }
    }
}
