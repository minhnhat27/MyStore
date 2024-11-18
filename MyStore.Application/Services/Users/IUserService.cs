using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch);
        Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch, RolesEnum role);
        Task<AddressDTO?> GetUserAddress(string userId);
        Task<AddressDTO?> UpdateOrCreateUserAddress(string userId, AddressDTO address);
        Task LockOut(string id, DateTimeOffset? endDate);
        Task<UserDTO> GetUserInfo(string userId);
        Task<UserInfo> UpdateUserInfo(string userId, UserInfo request);

        Task<IEnumerable<long>> GetFavorites(string userId);
        Task<PagedResponse<ProductDTO>> GetProductFavorites(string userId, PageRequest request);
        Task AddProductFavorite(string userId, long productId);
        Task DeleteProductFavorite(string userId, long productId);
    }
}
