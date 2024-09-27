using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task<CartItem?> SingleOrDefaultAsyncInclude(Expression<Func<CartItem, bool>> expression);
    }
}
