using MyStore.Application.DTO;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDTO>> GetOrders();
        Task<PagedResponse<OrderDTO>> GetOrders(int page, int pageSize, string? keySearch);
        Task<OrderDTO> GetOrder(int id);
        Task<OrderDetailResponse> GetOrderDetail(int id);
        Task<IEnumerable<OrderDTO>> GetOrdersByUserId(string userId);
        Task<OrderDTO> CreateOrder(string userId, OrderRequest request);
        Task<OrderDTO> UpdateOrder(int id, string userId, UpdateOrderRequest request);
        Task DeleteOrder(int id, string userId);
        Task CancelOrder(int id);
    }
}
