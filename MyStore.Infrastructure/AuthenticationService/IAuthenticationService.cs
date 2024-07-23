using MyStore.Application.DTO;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Infrastructure.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<JwtResponse> Login(LoginRequest request);
        Task<UserDTO> Register(RegisterRequest request);
        Task SendCode(string email);
        Task<JwtResponse> LoginGoogle(string token);
    }
}
