using MyStore.Application.DTO;

namespace MyStore.Application.Services.Payments
{
    public interface IPaymentService
    {
        Task<IEnumerable<string>> GetPaymentMethodsIsActive();
        Task<IEnumerable<string>> GetPaymentMethods();
    }
}
