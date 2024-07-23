using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository.Products
{
    public interface IProductSizeRepository : IRepository<ProductSize>
    {
        Task DeleteAllByProductIdAsync(int productId);
    }
}
