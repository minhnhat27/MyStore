using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository.Orders
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsIsActiveAsync();
    }
}
