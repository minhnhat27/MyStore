using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductColorRepository : Repository<ProductColor>, IProductColorRepository
    {
        public ProductColorRepository(MyDbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
