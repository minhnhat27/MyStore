using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductMaterialRepository : Repository<ProductMaterial>, IProductMaterialRepository
    {
        private readonly MyDbContext _dbContext;
        public ProductMaterialRepository(MyDbContext dbcontext) : base(dbcontext)
        {
            _dbContext = dbcontext;
        }
        //public async Task DeleteAllByProductIdAsync(int productId)
        //{
        //    var materials = await _dbContext.ProductMaterials.Where(e => e.ProductId == productId).ToListAsync();
        //    _dbContext.RemoveRange(materials);
        //    await _dbContext.SaveChangesAsync();
        //}
    }
}
