using MyStore.Application.ModelView;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Payments
{
    public interface IPaymentProcessor
    {
        string GetVNPayURL(Order order, DateTime deadline, string? orderDesc, string? ipAddress, string? locale = null);
        Task<string> GetPayOSURL(Order order, IEnumerable<OrderDetail> orderDetails);
    }
}
