using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> SingleOrDefaultAsync(Expression<Func<Order, bool>> expression);
    }
}
