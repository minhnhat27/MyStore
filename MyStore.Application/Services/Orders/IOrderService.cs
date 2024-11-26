using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<PagedResponse<OrderResponse>> GetOrdersByUserId(string userId, PageRequest request);
        Task<PagedResponse<OrderResponse>> GetOrdersByUserId(string userId, PageRequest request, DeliveryStatusEnum status);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId, string userId);
        Task<OrderDTO> UpdateOrder(long orderId, string userId, UpdateOrderRequest request);
        Task CancelOrder(long orderId, string userId);
        Task CancelOrder(long orderId);
        Task<CreateOrderResponse> CreateOrder(string userId, OrderRequest request);
        Task<string> Repayment(string userId, long orderId);

        Task NextOrderStatus(long orderId, DeliveryStatusEnum currentStatus);
        Task OrderToShipping(long orderId, OrderToShippingRequest request);

        Task ConfirmDelivery(long orderId, string userId);
        Task Review(long orderId, string userId, IEnumerable<ReviewRequest> reviews);

        Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch);
        Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch, DeliveryStatusEnum statusEnum);
        Task<OrderDetailsResponse> GetOrderDetail(long orderId);
        Task DeleteOrder(long orderId);

        Task SendEmailConfirmOrder(Order order, IEnumerable<OrderDetail> orderDetails);
    }
}
