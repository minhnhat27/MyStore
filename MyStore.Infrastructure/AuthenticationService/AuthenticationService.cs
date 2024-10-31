using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.ICaching;
using MyStore.Application.ISendMail;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MyStore.Application.DTOs;
using MyStore.Domain.Enumerations;
using MyStore.Application.IRepositories;

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
        private readonly ITransactionRepository _transaction;

        public AuthenticationService(UserManager<User> userManager,
            SignInManager<User> signInManager,
            ISendMailService sendMailService,
            ITransactionRepository transactionRepository,
            ICache cache, IConfiguration configuration, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _sendMailService = sendMailService;
            _cache = cache;
            _configuration = configuration;
            _mapper = mapper;
            _transaction = transactionRepository;
        }

        private async Task<string> CreateJwtToken(User user, DateTime exp, bool isRefreshToken = false)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Jti, user.Id),
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Email, user.Email ?? "")
                };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            if (isRefreshToken)
            {
                claims.Add(new Claim(ClaimTypes.Version, "Refresh Token"));
            }

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? ""));
            var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims,
                    expires: exp,
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }

        private async Task<User> GetUserExistsByUserName(string username)
        {
            var user = await _userManager.FindByNameAsync(username)
                ?? throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
            return user;
        }

        private async Task<User> GetUserExistsById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
            return user;
        }

        private async Task ThrowIfUserExists(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if(user != null)
            {
                throw new InvalidDataException(ErrorMessage.EMAIL_HAS_BEEN_REGISTERED);
            };
        }

        public async Task<JwtResponse> Login(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                var user = await GetUserExistsByUserName(username);

                var expires = DateTime.Now.AddHours(24);
                var accessToken = await CreateJwtToken(user, expires);
                //var refreshToken = await CreateJwtToken(user, true);

                return new JwtResponse
                {
                    AccessToken = accessToken,
                    //RefreshToken = refreshToken,
                    Expires = expires,
                    Fullname = user.Fullname,
                    Session = user.ConcurrencyStamp ?? user.Id,
                };
            }
            throw new InvalidDataException(ErrorMessage.INCORRECT_PASSWORD);
        }

        public async Task<JwtResponse> LoginGoogle(string credentials)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(credentials);

            var googleId = payload.Subject;
            var email = payload.Email;

            var provider = ExternalLoginEnum.GOOGLE.ToString();
            var user = await GetUserExistsByUserName(email);

            var result = await _signInManager.ExternalLoginSignInAsync(provider, googleId, false);
            if (!result.Succeeded)
            {
                var userInfo = new UserLoginInfo(provider, googleId, provider);
                await _userManager.AddLoginAsync(user, userInfo);
            }

            var expires = DateTime.Now.AddHours(24);
            var myAccessToken = await CreateJwtToken(user, expires);
            //var refreshToken = await CreateJwtToken(user, true);

            return new JwtResponse
            {
                AccessToken = myAccessToken,
                Expires = expires,
                //RefreshToken = refreshToken,
                Fullname = user.Fullname,
                Session = user.ConcurrencyStamp ?? user.Id
            };
        }

        public async Task<JwtResponse> LoginFacebook(string providerId)
        {
            var provider = ExternalLoginEnum.FACEBOOK.ToString();

            var user = await _userManager.FindByLoginAsync(provider, providerId)
                ?? throw new InvalidOperationException(ErrorMessage.NOT_REGISTERED);

            var result = await _signInManager.ExternalLoginSignInAsync(provider, providerId, false);
            if (!result.Succeeded)
            {
                throw new InvalidDataException(ErrorMessage.LOGIN_FAILD);
            }

            var expires = DateTime.Now.AddHours(24);
            var myAccessToken = await CreateJwtToken(user, expires);

            return new JwtResponse
            {
                AccessToken = myAccessToken,
                Expires = expires,
                Fullname = user.Fullname,
                Session = user.ConcurrencyStamp ?? user.Id
            };
        }

        public async Task LinkToFacebook(string userId, string providerId, string? name)
        {
            var user = await GetUserExistsById(userId);

            var provider = ExternalLoginEnum.FACEBOOK.ToString();
            var result = await _userManager.FindByLoginAsync(provider, providerId);
            if(result != null)
            {
                throw new ArgumentException("Tài khoản Facebook đã được liên kết với người dùng khác.");
            }
            await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerId, name));
        }
        
        public async Task UnlinkFacebook(string userId)
        {
            var user = await GetUserExistsById(userId);

            var provider = ExternalLoginEnum.FACEBOOK.ToString();

            var externalLogins = await _userManager.GetLoginsAsync(user);
            var facebookLogin = externalLogins.SingleOrDefault(e => e.LoginProvider == provider)
                ?? throw new InvalidOperationException("Tài khoản Facebook chưa được liên kết.");

            var result = await _userManager.RemoveLoginAsync(user, provider, facebookLogin.ProviderKey);
            if (!result.Succeeded)
            {
                throw new InvalidDataException(ErrorMessage.ERROR);
            }
        }

        public async Task<UserDTO> Register(RegisterRequest request)
        {
            try
            {
                string username;
                GoogleJsonWebSignature.Payload? payload = null;
                if(request.Email != null)
                {
                    var registerCache = AuthTypeEnum.Register.ToString() + request.Email;
                    var code = _cache.Get<string>(registerCache);
                    if (code == null || !code.Equals(request.Token))
                    {
                        throw new Exception(ErrorMessage.INVALID_OTP);
                    }
                    username = request.Email;
                    _cache.Remove(registerCache);
                }
                else
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                    username = payload.Email;
                }

                var user = new User
                {
                    Email = username,
                    Fullname = request.Name,
                    UserName = username,
                    PhoneNumber = request.PhoneNumber,
                    NormalizedUserName = request.Email,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                };
                user.DeliveryAddress = new DeliveryAddress
                {
                    UserId = user.Id,
                    Name = user.Fullname,
                    PhoneNumber = user.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
                }
                if(request.Email == null && payload != null)
                {
                    var googleId = payload.Subject;
                    var provider = ExternalLoginEnum.GOOGLE.ToString();
                    var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, googleId, provider));
                    if (!addLoginResult.Succeeded)
                    {
                        throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
                    }
                }

                return _mapper.Map<UserDTO>(user);
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task CheckGoogleRegister(string credentials)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(credentials);
            var email = payload.Email;
            await ThrowIfUserExists(email);
        }

        //private string ConvertToVietnamPhoneNumber(string phoneNumber)
        //{
        //    string cleaned = Regex.Replace(phoneNumber, @"\D", "");

        //    if (cleaned.StartsWith("0"))
        //    {
        //        return "+84" + cleaned.Substring(1);
        //    }
        //    if (cleaned.StartsWith("84") && !cleaned.StartsWith("+84"))
        //    {
        //        return "+84" + cleaned.Substring(2);
        //    }
        //    if (cleaned.StartsWith("+84"))
        //    {
        //        return cleaned;
        //    }
        //    return phoneNumber;
        //}

        public async Task SendCodeToEmail(AuthTypeEnum authType, string email)
        {
            await ThrowIfUserExists(email);

            var code = new Random().Next(100000, 999999);
            _cache.Set(authType.ToString() + email, code.ToString(), TimeSpan.FromMinutes(30));

            var subject = code + " is your verification code";
            var body = $"Hi!<br/><br/>" +
                $"Your verification code is: {code}.<br/><br/>" +
                "Please complete the account verification process in 30 minutes.<br/><br/>" +
                "This is an automated email. Please do not reply to this email.";

            await _sendMailService.SendMailToOne(email, subject, body);
        }

        public void VerifyOTP(VerifyOTPRequest verifyOTPRequest)
        {
            var codeCache = _cache.Get<string>(verifyOTPRequest.Type.ToString() + verifyOTPRequest.Email);
            if(codeCache == null || !codeCache.Equals(verifyOTPRequest.Token))
            {
                throw new InvalidDataException(ErrorMessage.INVALID_OTP);
            }
        }

        //public async Task SendCodeToPhoneNumber(string phoneNumber)
        //{
        //    try
        //    {
        //        var user = await _userManager.FindByNameAsync(phoneNumber);
        //        if (user != null)
        //        {
        //            throw new Exception(ErrorMessage.EXISTED_USER);
        //        }

        //        var code = new Random().Next(100000, 999999);
        //        _cache.Set("Register " + phoneNumber, code.ToString(), TimeSpan.FromMinutes(30));

        //        string accountSid = _configuration["TWILIO:TWILIO_ACCOUNT_SID"] ?? "";
        //        string authToken = _configuration["TWILIO:TWILIO_AUTH_TOKEN"] ?? "";
        //        string myPhoneNumber = _configuration["TWILIO:TWILIO_PHONE_NUMBER"] ?? "";

        //        TwilioClient.Init(accountSid, authToken);
        //        var message = await MessageResource.CreateAsync(
        //            body: "Mã xác thực VOA Store của bạn là: " + code,
        //            from: new PhoneNumber(myPhoneNumber),
        //            to: new PhoneNumber(ConvertToVietnamPhoneNumber(phoneNumber)));
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}

        public async Task ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
            
            if(currentPassword.Equals(newPassword))
            {
                throw new InvalidOperationException(ErrorMessage.DUPLICATE_CURRENT_PASSWORD);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, currentPassword, false);
            if (!result.Succeeded)
            {
                throw new InvalidDataException(ErrorMessage.INVALID_PASSWORD);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!changePasswordResult.Succeeded)
            {
                throw new InvalidOperationException(ErrorMessage.ERROR);
            }
        }

        public async Task<bool> CheckPassword(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

        public async Task ChangeEmail(string userId, string newEmail, string token)
        {
            var user = await _userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException(ErrorMessage.USER_NOT_FOUND);
            var changeEmailCache = AuthTypeEnum.ChangeEmail.ToString() + newEmail;

            var code = _cache.Get<string>(changeEmailCache);
            if (code == null || !code.Equals(token))
            {
                throw new InvalidDataException(ErrorMessage.INVALID_OTP);
            }

            using var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                var result = await _userManager.SetEmailAsync(user, newEmail);
                var resultUserName = await _userManager.SetUserNameAsync(user, newEmail);

                if (!result.Succeeded || !resultUserName.Succeeded)
                {
                    throw new Exception(ErrorMessage.ERROR);
                }
                var provider = ExternalLoginEnum.GOOGLE.ToString();

                var externalLogins = await _userManager.GetLoginsAsync(user);
                var googleLogin = externalLogins.SingleOrDefault(e => e.LoginProvider == provider);
                if(googleLogin != null)
                {
                    var resl = await _userManager.RemoveLoginAsync(user, provider, googleLogin.ProviderKey);
                    if (!resl.Succeeded)
                    {
                        throw new Exception(ErrorMessage.ERROR);
                    }
                }

                _cache.Remove(changeEmailCache);
                await _transaction.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }
    }
}
