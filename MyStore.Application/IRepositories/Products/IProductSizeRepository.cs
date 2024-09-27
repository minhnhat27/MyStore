using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductSizeRepository : IRepository<ProductSize>
    {
        Task<ProductSize> SingleAsyncInclude(Expression<Func<ProductSize, bool>> expression);
    }
}
