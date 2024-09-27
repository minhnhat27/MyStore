using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
using MyStore.Application.IRepositories.Users;
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
        private readonly IDeliveryAddressRepository _deliveryAddressRepository;

        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, 
            IUserRepository userRepository, IMapper mapper, IDeliveryAddressRepository deliveryAddressRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _deliveryAddressRepository = deliveryAddressRepository;
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

        public async Task<AddressDTO?> GetUserAddress(string userId)
        {
            var delivery = await _deliveryAddressRepository.SingleOrDefaultAsync(e => e.UserId == userId);
            if (delivery != null)
            {
                return _mapper.Map<AddressDTO>(delivery);
            }
            return null;
        }

        public async Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address)
        {
            try
            {
                var delivery = await _deliveryAddressRepository.SingleOrDefaultAsync(e => e.UserId == userId);
                if (delivery != null)
                {
                    delivery.Province_id = address.Province_id;
                    delivery.Province_name = address.Province_name;
                    delivery.District_id = address.District_id;
                    delivery.District_name = address.District_name;
                    delivery.Ward_id = address.Ward_id;
                    delivery.Ward_name = address.Ward_name;
                    delivery.Detail = address.Detail;

                    delivery.Name = address.Name;
                    delivery.PhoneNumber = address.PhoneNumber;

                    await _deliveryAddressRepository.UpdateAsync(delivery);
                    return _mapper.Map<AddressDTO?>(delivery);
                }
                throw new ArgumentException(ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
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
