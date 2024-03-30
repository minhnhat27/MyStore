using MyStore.Application.Request;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        public AccountService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<HttpResponseMessage> Login(LoginRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<HttpResponseMessage> Register(RegisterRequest request)
        {
            try
            {
                var exised = await _userRepository.GetUserByEmail(request.Email);
                if (exised != null)
                {
                    return new HttpResponseMessage(HttpStatusCode.BadRequest);
                }
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    Fullname = request.Name,
                    UserName = request.Email,
                    NormalizedUserName = request.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                };
                var result = await _userRepository.CreateUserAsync(user, request.Password);
                if(!result.Succeeded)
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent(ex.Message)
                };
            }
        }
    }
}
