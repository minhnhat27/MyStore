using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Repositories
{
    public class UserRespository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        public UserRespository(UserManager<User> user)
        {
            _userManager = user;
        }

        public async Task<List<User>> GetAll()
        {
            return await _userManager.Users.ToListAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<User?> GetUserByUsername(string username)
        {
            return await _userManager.FindByNameAsync(username);
        }

        public async Task<IdentityResult> CreateUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            return await _userManager.UpdateAsync(user);
        }
    }
}
