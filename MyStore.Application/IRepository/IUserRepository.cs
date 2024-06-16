using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize);
        Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string key);
        Task<int> CountAsync();
        Task<int> CountAsync(string key);
    }
}
