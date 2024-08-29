using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task DeleteRangeByUserId(string userId, IEnumerable<int> productIds);
    }
}
