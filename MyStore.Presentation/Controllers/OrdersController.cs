using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Services.Orders;

namespace MyStore.Presentation.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        public OrdersController(IOrderService orderService) => _orderService = orderService;
 
        [HttpGet("getOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetOrders([FromQuery] PageRequest request)
        {
            return Ok(await _orderService.GetOrdersAsync(request.Page, request.PageSize, request.Key));
        }
    }
}
