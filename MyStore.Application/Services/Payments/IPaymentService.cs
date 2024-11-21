using MyStore.Application.DTOs;
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
        Task VNPayCallback(VNPayRequest request);
        Task PayOSCallback(PayOSRequest request);
    }
}
