using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository.Products
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize, string key);
        Task<int> CountAsync(string key);
        Task<Product?> SingleOrDefaultAsync(int id);
    }
}
