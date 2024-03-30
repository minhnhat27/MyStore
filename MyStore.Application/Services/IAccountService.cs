using MyStore.Application.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services
{
    public interface IAccountService
    {
        Task<HttpResponseMessage> Login(LoginRequest request);
        Task<HttpResponseMessage> Register(RegisterRequest request);
    }
}
