using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductMaterialRepository(MyDbContext dbcontext) : Repository<ProductMaterial>(dbcontext), IProductMaterialRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
    }
}
