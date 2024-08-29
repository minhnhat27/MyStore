using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductMaterialRepository : IRepository<ProductMaterial>
    {
        Task DeleteAllByProductIdAsync(int productId);
    }
}
