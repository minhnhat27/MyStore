using MyStore.Application.IRepositories.Orders;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderRepository(MyDbContext context) : Repository<Order>(context), IOrderRepository
    {
        private readonly MyDbContext _dbContext = context;
    }
}
