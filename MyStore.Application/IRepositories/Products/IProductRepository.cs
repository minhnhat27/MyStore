using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> SingleOrDefaultAsyncInclude(Expression<Func<Product, bool>> expression);
    }
}
