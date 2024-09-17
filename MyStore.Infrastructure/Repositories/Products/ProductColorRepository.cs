using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductColorRepository : Repository<ProductColor>, IProductColorRepository
    {
        private readonly MyDbContext _dbContext;
        public ProductColorRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }

        public async Task<ProductColor> SingleAsync(int id)
        {
            return await _dbContext.ProductColors
                .Include(e => e.ProductSizes)
                .SingleAsync(e => e.Id == id);
        }
    }
}
