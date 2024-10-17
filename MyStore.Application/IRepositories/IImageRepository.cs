using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface IImageRepository : IRepository<Image>
    {
        Task<Image?> GetFirstImageByProductIdAsync(long id);
        Task<IEnumerable<Image>> GetImageByProductIdAsync(long productId);
    }
}
