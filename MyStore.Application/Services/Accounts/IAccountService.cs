using MyStore.Application.Request;
using MyStore.Application.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services.Accounts
{
    public interface IAccountService
    {
        Task<JwtResponse?> Login(LoginRequest request);
        Task<ApiResponse> Register(RegisterRequest request);
        Task<ApiResponse> SendCode(StringRequest request);
        Task<JwtResponse?> LoginGoogle(StringRequest request);
    }
}
