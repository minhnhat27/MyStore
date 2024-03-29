using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Domain.Entities;
using MyStore.Persistence.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Persistence.Repositories
{
    internal class UserRespository : IUserRepository
    {
        private readonly ApplicationContext _dbContext;
        private readonly UserManager<User> _userManager;
        public UserRespository(ApplicationContext context, UserManager<User> user)
        {
            _dbContext = context;
            _userManager = user;
        }

        public async Task<IdentityResult> CreateUser(User user)
        {
            return await _userManager.CreateAsync(user);
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
    }
}
