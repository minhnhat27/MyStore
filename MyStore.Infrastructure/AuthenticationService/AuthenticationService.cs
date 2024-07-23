using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.DTO;
using MyStore.Application.ICaching;
using MyStore.Application.ISendMail;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyStore.Infrastructure.AuthenticationService
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ISendMailService _sendMailService;
        private readonly ICache _cache;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        public AuthenticationService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISendMailService sendMailService,
            ICache cache, IConfiguration configuration, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendMailService = sendMailService;
            _cache = cache;
            _configuration = configuration;
            _mapper = mapper;
        }

        private async Task<string> CreateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, user.Id),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim(ClaimTypes.Name, user.Fullname ?? "")
                };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? ""));
            var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(12),
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        public async Task<JwtResponse> Login(LoginRequest request)
        {
            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    var accessToken = await CreateJwtToken(user);

                    return new JwtResponse
                    {
                        AccessToken = accessToken,
                        Email = user.Email,
                        FullName = user.Fullname,
                    };
                }
                else throw new Exception(ErrorMessage.USER_NOT_FOUND);
            }
            throw new ArgumentException(ErrorMessage.INCORRECT_PASSWORD);
        }

        public async Task<JwtResponse> LoginGoogle(string token)
        {
            try
            {
                GoogleJsonWebSignature.Payload google =
                    await GoogleJsonWebSignature.ValidateAsync(token);

                string provider = "Google";
                var user = await _userManager.FindByEmailAsync(google.Email);
                if (user == null)
                {
                    throw new Exception(ErrorMessage.USER_NOT_FOUND);
                }

                var result = await _signInManager.ExternalLoginSignInAsync(provider, google.Subject, false);
                if (!result.Succeeded)
                {
                    var userInfo = new UserLoginInfo(provider, provider, google.Subject);
                    await _userManager.AddLoginAsync(user, userInfo);
                }
                var accessToken = await CreateJwtToken(user);
                return new JwtResponse
                {
                    AccessToken = accessToken,
                    Email = user.Email,
                    FullName = user.Fullname,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserDTO> Register(RegisterRequest request)
        {
            var code = _cache.Get<string>("Register " + request.Email);
            if (code != null && code.Equals(request.VerifyCode))
            {
                var user = new User
                {
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
                    throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
                }
                _cache.Remove("Register " + request.Email);
                return _mapper.Map<UserDTO>(user);
            }
            else throw new Exception(ErrorMessage.INVALID_TOKEN);
        }

        public async Task SendCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                throw new Exception(ErrorMessage.EXISTED_USER);
            }

            var code = new Random().Next(100000, 999999);
            _cache.Set("Register " + email, code.ToString());

            var subject = code.ToString() + " is your verification code";
            var body = $"Hi!<br/><br/>" +
                $"Your verification code is: {code}.<br/><br/>" +
                "Please complete the account verification process in 30 minutes.<br/><br/>" +
                "This is an automated email. Please do not reply to this email.";

            await _sendMailService.SendMailToOne(email, subject, body);
        }
    }
}
