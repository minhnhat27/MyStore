using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepository.Orders
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetPagedAsync(int page, int size, string keySearch);
        Task<int> CountAsync(string keySearch);
        Task<Order?> SingleOrDefaultAsync(Expression<Func<Order, bool>> expression);
    }
}
