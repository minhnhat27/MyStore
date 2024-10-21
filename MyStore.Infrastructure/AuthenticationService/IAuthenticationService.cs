using MyStore.Application.DTOs;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Enumerations;

namespace MyStore.Infrastructure.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<JwtResponse> Login(string username, string password);
        Task<UserDTO> Register(RegisterRequest request);

        Task SendCodeToEmail(AuthTypeEnum authType, string email);
        //Task SendCodeToPhoneNumber(string phoneNumber);
        void VerifyOTP(VerifyOTPRequest verifyOTPRequest);

        Task<bool> CheckPassword(string userId, string password);
        Task ChangePassword(string userId, string currentPassword, string newPassword);
        Task ChangeEmail(string userId, string newEmail, string token);

        Task<JwtResponse> LoginGoogle(string token);
        Task<JwtResponse> LoginFacebook(string token);
    }
}
