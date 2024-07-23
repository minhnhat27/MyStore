using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, string key);
        Task<int> CountAsync(string key);
    }
}
