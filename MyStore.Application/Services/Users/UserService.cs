using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.IRepositories.Users;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

namespace MyStore.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IDeliveryAddressRepository _deliveryAddressRepository;

        private readonly IProductFavoriteRepository _productFavoriteRepository;

        private readonly IMapper _mapper;

        public UserService(UserManager<User> userManager, 
            IUserRepository userRepository, IMapper mapper, 
            IDeliveryAddressRepository deliveryAddressRepository, 
            IProductFavoriteRepository productFavoriteRepository)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _mapper = mapper;
            _deliveryAddressRepository = deliveryAddressRepository;
            _productFavoriteRepository = productFavoriteRepository;
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
                    e.Id.Equals(keySearch)
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

        public async Task<AddressDTO?> UpdateOrCreateUserAddress(string userId, AddressDTO address)
        {
            var delivery = await _deliveryAddressRepository.SingleOrDefaultAsync(e => e.UserId == userId);

            if (delivery == null)
            {
                delivery = new DeliveryAddress { UserId = userId, Name = address.Name };
                await _deliveryAddressRepository.AddAsync(delivery);
            }

            delivery.ProvinceID = address.ProvinceID;
            delivery.ProvinceName = address.ProvinceName;
            delivery.DistrictID = address.DistrictID;
            delivery.DistrictName = address.DistrictName;
            delivery.WardID = address.WardID;
            delivery.WardName = address.WardName;
            delivery.Detail = address.Detail;
            delivery.Name = address.Name;
            delivery.PhoneNumber = address.PhoneNumber;

            await _deliveryAddressRepository.UpdateAsync(delivery);
            return _mapper.Map<AddressDTO?>(delivery);
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

        private string MaskEmail(string email)
        {
            var emailParts = email.Split('@');
            if (emailParts.Length != 2)
            {
                throw new ArgumentException("Email không hợp lệ");
            }

            string name = emailParts[0];
            string domain = emailParts[1];

            int visibleChars = name.Length < 5 ? 2 : 5;
            string maskedName = name[..visibleChars].PadRight(name.Length, '*');

            return $"{maskedName}@{domain}";
        }

        public async Task<UserDTO> GetUserInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var res = _mapper.Map<UserDTO>(user);
                res.Email = res.Email != null ? MaskEmail(res.Email) : "";

                var loginProvider = await _userManager.GetLoginsAsync(user);
                res.Facebook = loginProvider
                    .SingleOrDefault(e => e.LoginProvider == ExternalLoginEnum.FACEBOOK.ToString())?.ProviderDisplayName;

                return res;
            }
            throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
        }

        public async Task<UserInfo> UpdateUserInfo(string userId, UserInfo request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Fullname = request.Fullname;
                user.PhoneNumber = request.PhoneNumber;
                await _userManager.UpdateAsync(user);

                return _mapper.Map<UserInfo>(user);
            }
            throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
        }

        public async Task<IEnumerable<long>> GetFavorites(string userId)
        {
            var favorites = await _productFavoriteRepository.GetAsync(e => e.UserId == userId);
            return favorites.Select(e => e.ProductId);
        }
        public async Task<PagedResponse<ProductDTO>> GetProductFavorites(string userId, PageRequest page)
        {
            var favorites = await _productFavoriteRepository
                .GetPagedAsync(page.Page, page.PageSize, e => e.UserId == userId, e => e.CreatedAt);

            var total = await _productFavoriteRepository.CountAsync(e => e.UserId == userId);

            var products = favorites.Select(e => e.Product).ToList();

            var items = _mapper.Map<IEnumerable<ProductDTO>>(products).Select(x =>
            {
                var image = products.Single(e => e.Id == x.Id).Images.FirstOrDefault();
                if (image != null)
                {
                    x.ImageUrl = image.ImageUrl;
                }
                return x;
            });

            return new PagedResponse<ProductDTO>
            {
                Items = items,
                Page = page.Page,
                PageSize = page.PageSize,
                TotalItems = total
            };
        }

        public async Task AddProductFavorite(string userId, long productId)
        {
            var favorites = new ProductFavorite
            {
                UserId = userId,
                ProductId = productId,
            };
            await _productFavoriteRepository.AddAsync(favorites);
        }

        public async Task DeleteProductFavorite(string userId, long productId)
            => await _productFavoriteRepository.DeleteAsync(userId, productId);
    }
}
