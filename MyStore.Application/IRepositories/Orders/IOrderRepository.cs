using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> SingleOrDefaultAsyncInclude(Expression<Func<Order, bool>> expression);
        Task<IEnumerable<Order>> GetPagedOrderByDescendingAsyncInclude<TKey>(int page, int pageSize, Expression<Func<Order, bool>>? expression, Expression<Func<Order, TKey>> orderByDesc);
    }
}
