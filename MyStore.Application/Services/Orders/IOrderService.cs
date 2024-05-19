using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task CreateOrderAsync(CreateOrderRequest request);
        Task<List<OrderResponse>> GetOrdersAsync();
        Task<List<OrderResponse>> GetOrdersAsync(int page, int pageSize);
        Task<OrderResponse?> GetOrderAsync(int id);
        Task<List<OrderResponse>> GetProductsByUserIdAsync(string userId);
        Task<bool> UpdateProductAsync(UpdateOrderRequest request);
        Task<bool> DeleteProductAsync(int id);
    }
}
