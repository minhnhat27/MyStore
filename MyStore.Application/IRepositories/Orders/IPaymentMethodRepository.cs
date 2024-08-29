using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories.Orders
{
    public interface IPaymentMethodRepository : IRepository<PaymentMethod>
    {
        Task<IEnumerable<PaymentMethod>> GetPaymentMethodsIsActiveAsync();
    }
}
