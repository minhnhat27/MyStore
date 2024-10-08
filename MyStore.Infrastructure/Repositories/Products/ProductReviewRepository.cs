using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductReviewRepository(MyDbContext dbcontext) : Repository<ProductReview>(dbcontext), IProductReviewRepository
    {
    }
}
