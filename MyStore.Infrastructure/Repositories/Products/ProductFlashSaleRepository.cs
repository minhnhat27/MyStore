using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductFlashSaleRepository(MyDbContext dbcontext) : Repository<ProductFlashSale>(dbcontext), IProductFlashSaleRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public override async Task<IEnumerable<ProductFlashSale>> GetAsync(Expression<Func<ProductFlashSale, bool>> expression)
        {
            return await _dbContext.ProductFlashSales
                .Where(expression)
                .Include(e => e.Product)
                    .ThenInclude(e => e.Images)
                .ToArrayAsync();
        }
    }
}
