using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly MyDbContext _dbContext;
        public OrderRepository(MyDbContext context) : base(context) => _dbContext = context;

        public async Task<IEnumerable<Order>> GetPagedAsync(int page, int size, string keySearch)
        {
            return await _dbContext.Orders
                .Where(e => e.Id.ToString().Equals(keySearch)
                    || e.OrderStatusName.Contains(keySearch)
                    || e.PaymentMethodName.Equals(keySearch))
                .Paginate(page, size)
                .ToListAsync();
        }

        public async Task<int> CountAsync(string keySearch)
        {
            return await _dbContext.Orders
                .Where(e => e.Id.ToString().Equals(keySearch)
                    || e.OrderStatusName.Contains(keySearch)
                    || e.PaymentMethodName.Equals(keySearch))
                .CountAsync();
        }

        public async Task<Order?> SingleOrDefaultAsync(Expression<Func<Order, bool>> expression)
            => await _dbContext.Orders.SingleOrDefaultAsync(expression);
    }
}
