using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
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
    }
}
