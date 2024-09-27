using MyStore.Application.Admin.Request;
using MyStore.Application.DTO;
using MyStore.Application.ModelView;
using MyStore.Application.Request;

namespace MyStore.Application.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethods();
        Task<string?> IsActivePaymentMethod(int id);
        Task<PaymentMethodDTO> UpdatePaymentMethod(int id, UpdatePaymentMethodRequest request);
        Task<PaymentMethodDTO> CreatePaymentMethod(CreatePaymentMethodRequest request);
        Task DeletePaymentMethod(int id);
        string GetVNPayURL(OrderInfo order, string ipAddress, string? locale = null);
        string Payback(string orderId);
        Task VNPayCallback(VNPayRequest request);
    }
}
