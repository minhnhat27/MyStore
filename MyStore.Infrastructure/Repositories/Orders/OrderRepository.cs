using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderRepository(MyDbContext context) : Repository<Order>(context), IOrderRepository
    {
        private readonly MyDbContext _dbContext = context;

        public Task<Order?> SingleOrDefaultAsyncInclude(Expression<Func<Order, bool>> expression)
        {
            return _dbContext.Orders
                .Include(e => e.OrderDetails)
                .SingleOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<Order>> GetPagedOrderByDescendingAsyncInclude<TKey>(int page, int pageSize, Expression<Func<Order, bool>>? expression, Expression<Func<Order, TKey>> orderByDesc)
        => expression == null
            ? await _dbContext.Orders
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.OrderDetails)
                .ToArrayAsync()
            : await _dbContext.Orders
                .Where(expression)
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.OrderDetails).ToArrayAsync();
    }
}
