using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class CartItemRepository(MyDbContext dbcontext) : Repository<CartItem>(dbcontext), ICartItemRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public override async Task<IEnumerable<CartItem>> GetAsync(Expression<Func<CartItem, bool>> expression)
        {
            return await _dbContext.CartItems
                .Where(expression)
                .Include(e => e.Product)
                    .ThenInclude(e => e.ProductColors)
                    .ThenInclude(e => e.ProductSizes)
                    .ThenInclude(e => e.Size)
                .AsSingleQuery().ToListAsync();
        }

        public async Task<CartItem?> SingleOrDefaultAsyncInclude(Expression<Func<CartItem, bool>> expression)
        {
            return await _dbContext.CartItems
                .Include(e => e.Product)
                    .ThenInclude(e => e.ProductColors)
                    .ThenInclude(e => e.ProductSizes)
                    .ThenInclude(e => e.Size)
                .AsSingleQuery()
                .SingleOrDefaultAsync(expression);
        }
    }
}
