using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductColorRepository(MyDbContext dbcontext) : Repository<ProductColor>(dbcontext), IProductColorRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
    }
}
