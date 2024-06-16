using MyStore.Application.Response;

namespace MyStore.Application.Services.Users
{
    public interface IUserService
    {
        Task<PagedResponse<UserResponse>> GetAllUsersAsync(int page, int pageSize, string? keySearch);
        Task<UserResponse> GetUserByIdAsync(int id);
        Task<UserResponse> GetUserByEmailAsync(string email);
    }
}
