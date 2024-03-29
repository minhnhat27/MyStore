using Microsoft.AspNetCore.Identity;
using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Persistence.Repositories
{
    internal interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task<User?> GetUserById(string id);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserByUsername(string username);
        Task<IdentityResult> CreateUser(User user);
    }
}
