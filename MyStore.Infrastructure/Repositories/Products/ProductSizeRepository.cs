using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductSizeRepository(MyDbContext dbcontext) : Repository<ProductSize>(dbcontext), IProductSizeRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public async Task<ProductSize> SingleAsyncInclude(Expression<Func<ProductSize, bool>> expression)
        {
            return await _dbContext.ProductSizes
                .Include(e => e.ProductColor)
                .SingleAsync(expression);
        }
    }
}
