using MyStore.Application.Admin.Response;
using MyStore.Application.DTO;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch);
        Task<UserDTO> GetUserByIdAsync(string userId);
        Task<AddressDTO?> GetUserAddress(string userId);
        Task<AddressDTO?> UpdateUserAddress(string userId, AddressDTO address);
        Task LockOut(string id, DateTimeOffset? endDate);
    }
}
