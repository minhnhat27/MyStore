﻿using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

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
    }
}
