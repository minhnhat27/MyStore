using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductRepository : IRepository<Product>
    {
        //Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize,
        //    Expression<Func<Product, bool>> filters, SortEnum sorter);
        //Task<int> CountAsync(Expression<Func<Product, bool>> filters, Expression<Func<Product, bool>> sorter);
    }
}
