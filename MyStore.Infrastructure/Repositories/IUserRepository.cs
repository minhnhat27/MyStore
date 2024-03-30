using Microsoft.AspNetCore.Identity;
using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task<User?> GetUserById(string id);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByUsername(string username);
        Task<IdentityResult> CreateUserAsync(User user, string password);
        Task<IdentityResult> UpdateUserAsync(User user);
    }
}
