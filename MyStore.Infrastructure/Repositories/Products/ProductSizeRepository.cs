using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductSizeRepository : Repository<ProductSize>, IProductSizeRepository
    {
        private readonly MyDbContext _dbContext;
        public ProductSizeRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task DeleteAllByProductIdAsync(int productId)
        {
            var products = await _dbContext.ProductSizes.Where(e => e.ProductId == productId).ToListAsync();
            _dbContext.ProductSizes.RemoveRange(products);
            await _dbContext.SaveChangesAsync();
        }
    }
}
