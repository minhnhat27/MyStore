using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
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
    }
}
