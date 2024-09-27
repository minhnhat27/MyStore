using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class PaymentMethodRepository(MyDbContext dbcontext) : Repository<PaymentMethod>(dbcontext), IPaymentMethodRepository
    {
        private readonly MyDbContext _dbcontext = dbcontext;
        public override async Task<IEnumerable<PaymentMethod>> GetAllAsync()
            => await _dbcontext.PaymentMethods.OrderBy(p => p.Id).ToListAsync();
        
    }
}
