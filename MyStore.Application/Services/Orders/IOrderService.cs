using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderResponse>> GetOrdersByUserId(string userId, PageRequest request);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId, string userId);
        Task<OrderDTO> UpdateOrder(long orderId, string userId, UpdateOrderRequest request);
        Task CancelOrder(long orderId, string userId);
        Task CancelOrder(long orderId);
        Task<string?> CreateOrder(string userId, OrderRequest request);

        Task NextOrderStatus(long orderId, DeliveryStatusEnum currentStatus);
        Task OrderToShipping(long orderId, OrderToShippingRequest request);

        Task ConfirmDelivery(long orderId, string userId);
        Task Review(long orderId, string userId, IEnumerable<ReviewRequest> reviews);

        Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch);
        Task<PagedResponse<OrderDTO>> GetWithOrderStatus(DeliveryStatusEnum statusEnum, PageRequest request);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId);
        Task DeleteOrder(long orderId);
    }
}
