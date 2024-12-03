using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductFeatureRepository(MyDbContext dbcontext) : Repository<ProductFeature>(dbcontext), IProductFeatureRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public async Task DeleteAll()
        {
            await _dbContext.ProductFeatures.ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();
        }
    }
}
