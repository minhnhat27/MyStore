using Microsoft.AspNetCore.Identity;
using MyStore.Domain.Entities;

namespace MyStore.Application.IRepository
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetUserByIdAsync(string id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> UpdateUserAsync(User user);
        Task<SignInResult> LoginAsync(string username, string password);
        Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey);
        Task<IdentityResult> AddLoginAsync(User user, string loginProvider, string providerKey);
        Task<IList<string>> GetRolesAsync(User user);
    }
}
