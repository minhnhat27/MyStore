using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.Request;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.Caching;
using MyStore.Infrastructure.Email;
using MyStore.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISendMailService _sendMailService;
        private readonly ICodeCaching _codeCaching;
        private readonly IConfiguration _configuration;
        public AccountService(IUserRepository userRepository, ISendMailService sendMailService,
            ICodeCaching codeCaching, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _sendMailService = sendMailService;
            _codeCaching = codeCaching;
            _configuration = configuration;
        }

        private async Task<string> CreateJwtToken(User user)
        {
            var roles = await _userRepository.GetRolesAsync(user);
            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Email, user.Email ?? ""),
                        new Claim(ClaimTypes.Name, user.Fullname ?? "")
                    };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SerectKey"]!));
            var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
                );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public async Task<HttpResponseMessage> Login(LoginRequest request)
        {
            try
            {
                var result = await _userRepository.LoginAsync(request.Email, request.Password);
                if (result.Succeeded)
                {
                    var user = await _userRepository.GetUserByEmailAsync(request.Email);
                    if(user == null)
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.BadRequest,
                            Content = new StringContent("User not found")
                        };
                    }
                    var jwtToken = await CreateJwtToken(user);
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Accepted,
                        Content = new StringContent(jwtToken)
                    };
                }
                else
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Email or password is incorrect")
                    };
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ExpectationFailed,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        public async Task<HttpResponseMessage> Register(RegisterRequest request)
        {
            try
            {
                var exised = await _userRepository.GetUserByEmailAsync(request.Email);
                if (exised != null)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Content = new StringContent("User is exised")
                    };
                }
                if (!_codeCaching.GetCodeFromEmail(request.Email).Equals(request.VerifyCode))
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.BadRequest,
                        Content = new StringContent("Verify code is incorrect")
                    };
                }
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    Fullname = request.Name,
                    UserName = request.Email,
                    NormalizedUserName = request.Email,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                };
                var result = await _userRepository.CreateUserAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.LengthRequired,
                        Content = new StringContent("Minimum 6 characters at least 1 uppercase letter, 1 lowercase letter and 1 number.")
                    };
                }
                _codeCaching.RemoveCode(request.Email);
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ExpectationFailed,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        public async Task<HttpResponseMessage> SendCode(StringRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Id);
                if(user != null)
                {
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.Conflict,
                        Content = new StringContent("User is exised")
                    };
                }

                _codeCaching.SetCodeForEmail(request.Id);
                var code = _codeCaching.GetCodeFromEmail(request.Id);

                var subject = code.ToString() + " is your verification code";
                var body = $"Hi!<br/><br/>" +
                    $"Your verification code is: {code}.<br/><br/>" +
                    "Please complete the account verification process in 30 minutes.<br/><br/>" +
                    "This is an automated email. Please do not reply to this email.";
                await _sendMailService.SendMailToOne(request.Id, subject, body);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.ExpectationFailed,
                    Content = new StringContent(ex.Message)
                };
            }
        }
    }
}
