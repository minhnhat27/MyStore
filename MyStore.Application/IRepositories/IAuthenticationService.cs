using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.IRepositories
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(string username, string password);
        Task<User> CreateUserAsync(string email, string password, string? name, string? phoneNumber);
        Task<UserDTO> Register(RegisterRequest request);

        Task SendCodeToEmail(AuthTypeEnum authType, string email);
        //Task SendCodeToPhoneNumber(string phoneNumber);
        void VerifyOTP(VerifyOTPRequest verifyOTPRequest);

        Task<bool> CheckPassword(string userId, string password);
        Task ChangePassword(string userId, string currentPassword, string newPassword);
        Task ChangeEmail(string userId, string newEmail, string token);

        Task ResetPassword(string email, string token, string password);

        Task<LoginResponse> LoginGoogle(string credentials);
        Task<LoginResponse> LoginFacebook(string token);
        Task LinkToFacebook(string userId, string providerId, string? name);
        Task UnlinkFacebook(string userId);

        Task CheckGoogleRegister(string credentials);

        Task<UserResponse> UpdateUserAccount(string userId, UpdateAccountRequest request);
        Task<UserResponse> CreateUserWithRoles(AccountRequest request);
    }
}
