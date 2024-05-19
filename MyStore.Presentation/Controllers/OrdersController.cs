using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyStore.Application.Request;
using MyStore.Application.Response;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders([FromQuery] PageRequest page)
        {
            var orders = new List<OrderResponse>
            {
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                },
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                },
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                },
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                },
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                },
                new OrderResponse
                {
                    Id = 124,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = false,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1202931
                },
                new OrderResponse
                {
                    Id = 2153,
                    OrderDate = DateTime.Now,
                    OrderStatus = "shipping",
                    Paid = true,
                    PaymentMethod = "COD",
                    UserId = "122",
                    Total = 1221455
                }

            };

            //await _orderService.GetAllAsync()
            return Ok(orders);
        }

    }
}
