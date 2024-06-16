using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        public UserService(UserManager<User> userManager, IUserRepository userRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
        }

        public async Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch)
        {
            int totalUsers;
            IEnumerable< User> users;
            if(keySearch == null)
            {
                totalUsers = await _userRepository.CountAsync();
                users = await _userRepository.GetAllUsersAsync(page, pageSize);
            }
            else
            {
                totalUsers = await _userRepository.CountAsync(keySearch);
                users = await _userRepository.GetAllUsersAsync(page, pageSize, keySearch);
            }
            
            var items = users
                .Select(e => new UserResponse
                {
                    Id = e.Id,
                    Email = e.Email,
                    EmailConfirmed = e.EmailConfirmed,
                    Fullname = e.Fullname,
                    PhoneNumber = e.PhoneNumber,
                    LockedOut = e.LockoutEnd > DateTime.Now,
                    LockoutEnd = e.LockoutEnd > DateTime.Now ? e.LockoutEnd : null,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                });
            return new PagedResponse<UserResponse>
            {
                Items = items,
                TotalItems = totalUsers,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<UserResponse> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
