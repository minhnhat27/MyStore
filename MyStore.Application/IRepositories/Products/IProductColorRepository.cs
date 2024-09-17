using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories.Products
{
    public interface IProductColorRepository : IRepository<ProductColor>
    {
        Task<ProductColor> SingleAsync(int id);
    }
}
