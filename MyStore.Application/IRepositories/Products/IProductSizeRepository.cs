using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductSizeRepository : IRepository<ProductSize>
    {
        Task DeleteAllByProductIdAsync(int productId);
    }
}
