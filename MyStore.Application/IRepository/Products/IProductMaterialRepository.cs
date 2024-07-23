using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository.Products
{
    public interface IProductMaterialRepository : IRepository<ProductMaterial>
    {
        Task DeleteAllByProductIdAsync(int productId);
    }
}
