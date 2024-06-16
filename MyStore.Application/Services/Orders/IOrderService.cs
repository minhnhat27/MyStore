using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task CreateOrderAsync(CreateOrderRequest request);
        Task<IEnumerable<OrderResponse>> GetOrdersAsync();
        Task<PagedResponse<OrderResponse>> GetOrdersAsync(int page, int pageSize, string? keySearch);
        Task<OrderResponse?> GetOrderAsync(int id);
        Task<IEnumerable<OrderResponse>> GetProductsByUserIdAsync(string userId);
        Task<bool> UpdateProductAsync(UpdateOrderRequest request);
        Task<bool> DeleteProductAsync(int id);
        Task<IEnumerable<string>> GetPaymentMethods();
    }
}
