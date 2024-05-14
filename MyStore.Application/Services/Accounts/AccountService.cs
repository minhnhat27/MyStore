using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.IRepository;
using MyStore.Application.IRepository.Caching;
using MyStore.Application.IRepository.SendMail;
using MyStore.Application.Request;
using MyStore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MyStore.Application.Services.Accounts
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

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? ""));
            var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(12),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
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
                    if (user == null)
                    {
                        return new HttpResponseMessage(HttpStatusCode.NotFound);
                    }
                    var jwtToken = await CreateJwtToken(user);
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(jwtToken)
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        public async Task<HttpResponseMessage> LoginGoogle(StringRequest request)
        {
            try
            {
                GoogleJsonWebSignature.Payload google =
                    await GoogleJsonWebSignature.ValidateAsync(request.Id);

                string provider = "Google";
                var user = await _userRepository.GetUserByEmailAsync(google.Email);
                if (user == null)
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                }

                var result = await _userRepository.ExternalLoginSignInAsync(provider, google.Subject);
                if (!result.Succeeded)
                {
                    await _userRepository.AddLoginAsync(user, provider, google.Subject);
                }
                var jwtToken = await CreateJwtToken(user);
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jwtToken)
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        public async Task<HttpResponseMessage> Register(RegisterRequest request)
        {
            try
            {
                var exist = await _userRepository.GetUserByEmailAsync(request.Email);
                if (exist != null)
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
                }
                if (!_codeCaching.GetCodeFromEmail(request.Email).Equals(request.VerifyCode))
                {
                    return new HttpResponseMessage(HttpStatusCode.ExpectationFailed);
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
                    //"Minimum 6 characters at least 1 uppercase letter, 1 lowercase letter and 1 number."
                    return new HttpResponseMessage(HttpStatusCode.LengthRequired);
                }
                _codeCaching.RemoveCode(request.Email);
                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }

        public async Task<HttpResponseMessage> SendCode(StringRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(request.Id);
                if (user != null)
                {
                    return new HttpResponseMessage(HttpStatusCode.Conflict);
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
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.Message)
                };
            }
        }
    }
}
