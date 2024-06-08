using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.ICaching;
using MyStore.Application.ISendMail;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MyStore.Application.Services.Accounts
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private readonly ISendMailService _sendMailService;

        private readonly ICodeCache _codeCaching;
        private readonly IConfiguration _configuration;
        public AccountService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISendMailService sendMailService,
            ICodeCache codeCaching, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendMailService = sendMailService;
            _codeCaching = codeCaching;
            _configuration = configuration;
        }

        private async Task<string> CreateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
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

        public async Task<JwtResponse?> Login(LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    throw new Exception();
                }
                var jwtToken = await CreateJwtToken(user);
                return new JwtResponse
                {
                    Jwt = jwtToken,
                    Email = user.Email,
                    FullName = user.Fullname,
                };
            }
            else
            {
                return null;
            }
        }

        public async Task<JwtResponse?> LoginGoogle(StringRequest request)
        {
            try
            {
                GoogleJsonWebSignature.Payload google =
                    await GoogleJsonWebSignature.ValidateAsync(request.Id);

                string provider = "Google";
                var user = await _userManager.FindByEmailAsync(google.Email);
                if (user == null)
                {
                    return null;
                }

                var result = await _signInManager.ExternalLoginSignInAsync(provider, google.Subject, false);
                if (!result.Succeeded)
                {
                    var userInfo = new UserLoginInfo(provider, provider, google.Subject);
                    await _userManager.AddLoginAsync(user, userInfo);
                }
                var jwtToken = await CreateJwtToken(user);
                return new JwtResponse
                {
                    Jwt = jwtToken,
                    Email = user.Email,
                    FullName = user.Fullname,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ApiResponse> Register(RegisterRequest request)
        {
            try
            {
                if (!_codeCaching.GetCodeFromEmail(request.Email).Equals(request.VerifyCode))
                {
                    return new ApiResponse { Success = false, Message = "Incorrect verification code" };
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
                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    string err = "";
                    foreach(var item in result.Errors)
                    {
                        err += item + "\n";
                    }
                    return new ApiResponse { Success = false, Message = err };
                }
                _codeCaching.RemoveCode(request.Email);
                return new ApiResponse { Success = true };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ApiResponse> SendCode(StringRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Id);
                if (user != null)
                {
                    return new ApiResponse { Success = false, Message = "User already exists" };
                }

                var code = _codeCaching.SetCodeForEmail(request.Id);

                var subject = code.ToString() + " is your verification code";
                var body = $"Hi!<br/><br/>" +
                    $"Your verification code is: {code}.<br/><br/>" +
                    "Please complete the account verification process in 30 minutes.<br/><br/>" +
                    "This is an automated email. Please do not reply to this email.";

                await _sendMailService.SendMailToOne(request.Id, subject, body);
                return new ApiResponse { Success = true };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
