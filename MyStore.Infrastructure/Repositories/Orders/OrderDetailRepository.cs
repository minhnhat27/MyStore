using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly MyDbContext _dbContext;
        public OrderDetailRepository(MyDbContext dbcontext) : base(dbcontext) => _dbContext = dbcontext;
        public override async Task<IEnumerable<OrderDetail>> GetAsync(Expression<Func<OrderDetail, bool>> expression)
        {
            return await _dbContext.OrderDetails
                .Include(e => e.Product)
                .Where(expression).ToListAsync();
        }
    }
}
