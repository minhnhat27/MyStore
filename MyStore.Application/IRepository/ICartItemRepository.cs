using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        Task DeleteRangeByUserId(string userId, IEnumerable<int> productIds);
    }
}
