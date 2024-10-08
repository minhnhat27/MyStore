using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> SingleOrDefaultAsyncInclude(Expression<Func<Order, bool>> expression);
    }
}
