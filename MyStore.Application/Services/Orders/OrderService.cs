using MyStore.Application.IRepository;
using MyStore.Application.Request;
using MyStore.Application.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        public OrderService(IOrderRepository orderRepository) => _orderRepository = orderRepository;
        public Task CreateOrderAsync(CreateOrderRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProductAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<List<OrderResponse>> GetOrdersAsync()
        {
            var result = await _orderRepository.GetOrdersAsync();
            return result.Select(e => new OrderResponse
            {
                Id = e.Id,
                OrderDate = e.OrderDate,
                OrderStatus = e.OrderStatusName,
                Paid = e.Paid,
                PaymentMethod = e.PaymentMethodName,
                UserId = e.UserId,
                Total = e.Total,
            }).ToList();
        }
        public async Task<List<OrderResponse>> GetOrdersAsync(int page, int pageSize)
        {
            var result = await _orderRepository.GetOrdersAsync(page, pageSize);
            return result.Select(e => new OrderResponse
            {
                Id = e.Id,
                OrderDate = e.OrderDate,
                OrderStatus = e.OrderStatusName,
                Paid = e.Paid,
                PaymentMethod = e.PaymentMethodName,
                UserId = e.UserId,
                Total = e.Total,
            }).ToList();
        }

        public Task<OrderResponse?> GetOrderAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<OrderResponse>> GetProductsByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateProductAsync(UpdateOrderRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
