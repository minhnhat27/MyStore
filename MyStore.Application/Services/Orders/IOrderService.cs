using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderDTO>> GetOrdersByUserId(string userId, PageRequest request);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId, string userId);
        Task<OrderDTO> UpdateOrder(long orderId, string userId, UpdateOrderRequest request);
        Task CancelOrder(long orderId, string userId);
        Task<string?> CreateOrder(string userId, OrderRequest request);

        Task Review(long orderId, string userId, ReviewProductRequest request);

        Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId);
        Task DeleteOrder(long orderId);
    }
}
