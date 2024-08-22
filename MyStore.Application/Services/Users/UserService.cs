using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
using MyStore.Application.IRepository;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, IUserRepository userRepository, IMapper mapper)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch)
        {
            int totalUsers;
            IEnumerable< User> users;
            if(string.IsNullOrEmpty(keySearch))
            {
                totalUsers = await _userRepository.CountAsync();
                users = await _userRepository.GetPagedOrderByDescendingAsync(page, pageSize, null, e => e.CreatedAt);
            }
            else
            {
                Expression<Func<User, bool>> expression = e =>
                    e.Id.Contains(keySearch)
                    || (e.Fullname != null && e.Fullname.Contains(keySearch))
                    || (e.Email != null && e.Email.Contains(keySearch))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(keySearch));

                totalUsers = await _userRepository.CountAsync(expression);
                users = await _userRepository.GetPagedOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }

            var items = _mapper.Map<IEnumerable<UserResponse>>(users).Select(e =>
            {
                e.LockedOut = e.LockoutEnd > DateTime.Now;
                e.LockoutEnd = e.LockoutEnd > DateTime.Now ? e.LockoutEnd : null;
                return e;
            });

            return new PagedResponse<UserResponse>
            {
                Items = items,
                TotalItems = totalUsers,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<UserDTO> GetUserByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return _mapper.Map<UserDTO>(user);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task LockOut(string id, DateTimeOffset? endDate)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (endDate != null)
                    user.LockoutEnd = endDate.Value.AddDays(1);
                else user.LockoutEnd = endDate;

                await _userManager.UpdateAsync(user);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
