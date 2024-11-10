using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Enumerations;

namespace MyStore.Infrastructure.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(string username, string password);
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
    }
}
