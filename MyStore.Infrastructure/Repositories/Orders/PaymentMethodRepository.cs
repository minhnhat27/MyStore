using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class PaymentMethodRepository : Repository<PaymentMethod>, IPaymentMethodRepository
    {
        private readonly MyDbContext _dbContext;
        public PaymentMethodRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }
        public async Task<IEnumerable<PaymentMethod>> GetPaymentMethodsIsActiveAsync()
        {
            return await _dbContext.PaymentMethods.Where(e => e.isActive).ToListAsync();
        }
    }
}
