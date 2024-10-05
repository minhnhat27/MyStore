using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderDetailRepository(MyDbContext dbcontext) : Repository<OrderDetail>(dbcontext), IOrderDetailRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public override async Task<IEnumerable<OrderDetail>> GetAsync(Expression<Func<OrderDetail, bool>> expression)
        {
            return await _dbContext.OrderDetails
                .Where(expression)
                .Include(e => e.Product)
                .ToListAsync();
        }
    }
}
