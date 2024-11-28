using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductFeatureRepository : IRepository<ProductFeature>
    {
        Task DeleteAll();
    }
}
