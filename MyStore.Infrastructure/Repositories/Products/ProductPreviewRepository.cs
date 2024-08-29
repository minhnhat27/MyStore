using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductPreviewRepository : Repository<ProductReview>, IProductPreviewRepository
    {
        public ProductPreviewRepository(MyDbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
