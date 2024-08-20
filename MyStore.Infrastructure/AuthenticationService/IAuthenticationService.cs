using MyStore.Application.DTO;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Infrastructure.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<JwtResponse> Login(string username, string password);
        Task<UserDTO> Register(RegisterRequest request);
        Task SendCodeToEmail(string email);
        Task SendCodeToPhoneNumber(string phoneNumber);
        void VerifyOTP(string email, string token);
        Task<JwtResponse> LoginGoogle(string token);
    }
}
