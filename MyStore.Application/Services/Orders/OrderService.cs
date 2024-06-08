using MyStore.Application.ICaching;
using MyStore.Application.IRepository;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICache _orderCache;
        public OrderService(IOrderRepository orderRepository, ICache cache)
        {
            _orderRepository = orderRepository;
            _orderCache = cache;
        }
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
        public async Task<PageResponse<OrderResponse>> GetOrdersAsync(int page, int pageSize, string? keySearch)
        {
            int totalOrder;
            IList<Order> orders;
            if (keySearch == null)
            {
                totalOrder = await _orderRepository.CountAsync();
                orders = await _orderRepository.GetOrdersAsync(page, pageSize);
            }
            else
            {
                totalOrder = await _orderRepository.CountAsync(keySearch);
                orders = await _orderRepository.GetOrdersAsync(page, pageSize, keySearch);
            }
            var items = orders.Select(e => new OrderResponse
            {
                Id = e.Id,
                OrderDate = e.OrderDate,
                OrderStatus = e.OrderStatusName,
                Paid = e.Paid,
                PaymentMethod = e.PaymentMethodName,
                UserId = e.UserId,
                Total = e.Total,
            }).ToList();

            return new PageResponse<OrderResponse>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalOrder
            };
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

        private async Task UpdateCachedPaymentMethods() 
            => _orderCache.Set("PaymentMethods", await _orderRepository.GetPaymentMethodsAsync());

        public async Task<IList<string>> GetPaymentMethods()
        {
            var payment = _orderCache.Get<IList<PaymentMethod>>("PaymentMethods");
            if(payment == null)
            {
                payment = await _orderRepository.GetPaymentMethodsAsync();
                _orderCache.Set("PaymentMethods", payment);
            }

            return payment.Select(e => e.Name).ToList();
        }
    }
}
