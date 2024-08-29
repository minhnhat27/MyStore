using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class CartItemRepository : Repository<CartItem>, ICartItemRepository
    {
        private readonly MyDbContext _dbContext;
        public CartItemRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }
        public async Task DeleteRangeByUserId(string userId, IEnumerable<int> productIds)
        {
            var cartItemsToDelete = await _dbContext.CartItems
               .Where(e => e.UserId == userId && productIds.Contains(e.ProductId))
               .ToListAsync();

            _dbContext.CartItems.RemoveRange(cartItemsToDelete);
            await _dbContext.SaveChangesAsync();
        }
        public override async Task<IEnumerable<CartItem>> GetAsync(Expression<Func<CartItem, bool>> expression)
        {
            return await _dbContext.CartItems
                .Include(e => e.Product)
                .ThenInclude(e => e.Images)
                .Where(expression).ToListAsync();
        }
    }
}
