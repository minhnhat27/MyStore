using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch);
        Task<OrderDTO> GetOrder(int id);
        Task<OrderDetailsResponse> GetOrderDetail(int id);
        Task<PagedResponse<OrderDTO>> GetOrdersByUserId(string userId, PageRequest request);
        Task<string?> CreateOrder(string userId, OrderRequest request);
        Task<OrderDTO> UpdateOrder(int id, string userId, UpdateOrderRequest request);
        Task DeleteOrder(int id, string userId);
        Task CancelOrder(int orderId, string userId);
    }
}
