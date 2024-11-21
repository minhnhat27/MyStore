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
using System.Data;
using System.Linq;

namespace MyStore.Infrastructure.Authentication
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

        private async Task<JwtResponse> CreateJwtToken(User user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var exps = DateTime.Now.AddHours(24);
            //var isAdmin = roles.Contains("Admin");

            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Jti, user.Id),
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Email, user.Email ?? ""),
                    new(ClaimTypes.Name, user.Fullname ?? "")
                };
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            //if (isRefreshToken)
            //{
            //    claims.Add(new Claim(ClaimTypes.Version, "Refresh Token"));
            //}

            var securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["JWT:SecretKey"] ?? ""));
            var jwtToken = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                    claims,
                    expires: exps,
                    signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256));

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            return new JwtResponse
            {
                AccessToken = accessToken,
                Expires = exps,
                Roles = roles
            };
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
            if (user != null)
            {
                throw new InvalidDataException(ErrorMessage.EMAIL_HAS_BEEN_REGISTERED);
            };
        }

        private void ThrowIfInvalidCode(AuthTypeEnum type, string key, string token)
        {
            var cache = type.ToString() + key;
            var code = _cache.Get<string>(cache);
            if (code == null || !code.Equals(token))
            {
                throw new InvalidDataException(ErrorMessage.INVALID_OTP);
            }
        }

        private void SetCache(AuthTypeEnum type, string key, string token, TimeSpan time)
        {
            var cache = type.ToString() + key;
            _cache.Set(cache, token, time);
        }

        private void RemoveCache(AuthTypeEnum type, string key, string token)
        {
            var cache = type.ToString() + key;
            _cache.Remove(cache);
        }

        public async Task<LoginResponse> Login(string username, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(username, password, false, false);
            if (result.Succeeded)
            {
                var user = await GetUserExistsByUserName(username);
                var accessToken = await CreateJwtToken(user);

                return new LoginResponse
                {
                    AccessToken = accessToken.AccessToken,
                    Roles = accessToken.Roles,
                    Expires = accessToken.Expires,
                    Fullname = user.Fullname,
                    Session = user.ConcurrencyStamp ?? user.Id,
                };
            }
            throw new InvalidDataException(ErrorMessage.INCORRECT_PASSWORD);
        }

        public async Task<LoginResponse> LoginGoogle(string credentials)
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

            var myAccessToken = await CreateJwtToken(user);

            return new LoginResponse
            {
                AccessToken = myAccessToken.AccessToken,
                Expires = myAccessToken.Expires,
                Roles = myAccessToken.Roles,
                //RefreshToken = refreshToken,
                Fullname = user.Fullname,
                Session = user.ConcurrencyStamp ?? user.Id
            };
        }

        public async Task<LoginResponse> LoginFacebook(string providerId)
        {
            var provider = ExternalLoginEnum.FACEBOOK.ToString();

            var user = await _userManager.FindByLoginAsync(provider, providerId)
                ?? throw new InvalidOperationException(ErrorMessage.NOT_REGISTERED);

            var result = await _signInManager.ExternalLoginSignInAsync(provider, providerId, false);
            if (!result.Succeeded)
            {
                throw new InvalidDataException(ErrorMessage.LOGIN_FAILD);
            }

            var myAccessToken = await CreateJwtToken(user);

            return new LoginResponse
            {
                AccessToken = myAccessToken.AccessToken,
                Expires = myAccessToken.Expires,
                Roles = myAccessToken.Roles,
                Fullname = user.Fullname,
                Session = user.ConcurrencyStamp ?? user.Id
            };
        }

        public async Task LinkToFacebook(string userId, string providerId, string? name)
        {
            var user = await GetUserExistsById(userId);

            var provider = ExternalLoginEnum.FACEBOOK.ToString();
            var result = await _userManager.FindByLoginAsync(provider, providerId);
            if (result != null)
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

        public async Task<User> CreateUserAsync(string email, string password, string? name, string? phoneNumber)
        {
            var user = new User
            {
                Email = email,
                NormalizedEmail = email.ToUpper(),
                Fullname = name,
                PhoneNumber = phoneNumber,
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
            }
            return user;
        }
        private async Task AddToRolesAsync(User user, IEnumerable<string> roles)
        {
            var result = await _userManager.AddToRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }
        private async Task AddToRoleAsync(User user, string role)
        {
            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(";", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task<UserDTO> Register(RegisterRequest request)
        {
            try
            {
                string email;
                GoogleJsonWebSignature.Payload? payload = null;
                if (request.Email != null)
                {
                    ThrowIfInvalidCode(AuthTypeEnum.Register, request.Email, request.Token);
                    email = request.Email;

                    RemoveCache(AuthTypeEnum.Register, request.Email, request.Token);
                }
                else
                {
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.Token);
                    email = payload.Email;
                }

                //var user = new User
                //{
                //    Email = username,
                //    Fullname = request.Name,
                //    UserName = username,
                //    PhoneNumber = request.PhoneNumber,
                //    NormalizedUserName = username.ToUpper(),
                //    EmailConfirmed = true,
                //    SecurityStamp = Guid.NewGuid().ToString(),
                //};

                var user = await CreateUserAsync(email, request.Password, request.Name, request.PhoneNumber);
                await AddToRoleAsync(user, RolesEnum.User.ToString());
                user.DeliveryAddress = new DeliveryAddress
                {
                    UserId = user.Id,
                    Name = user.Fullname,
                    PhoneNumber = user.PhoneNumber,
                };

                if (request.Email == null && payload != null)
                {
                    var googleId = payload.Subject;
                    var provider = ExternalLoginEnum.GOOGLE.ToString();
                    var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, googleId, provider));
                    if (!addLoginResult.Succeeded)
                    {
                        throw new Exception(string.Join(";", addLoginResult.Errors.Select(e => e.Description)));
                    }
                }
                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception)
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
            if (authType != AuthTypeEnum.ForgetPassword)
            {
                await ThrowIfUserExists(email);
            }
            var code = new Random().Next(100000, 999999).ToString();
            int expire = 30;
            SetCache(authType, email, code, TimeSpan.FromMinutes(expire));
            var storeName = _configuration["Store:Name"] ?? "Thông báo";

            var subject = storeName + " mã xác nhận của bạn.";
            string body;
            var path = _sendMailService.SendCodeEmailPath;
            if (!File.Exists(path))
            {
                body = $"{code} là mã xác nhận của bạn. Mã sẽ hết hạn sau {expire} phút.";
            }
            body = File.ReadAllText(path);
            body = body.Replace("{CODE}", code);
            body = body.Replace("{minute}", expire.ToString());
            //body = body.Replace("{IMAGE}", Path.Combine(host ?? "", "Logo-t-1x1.png"));

            await _sendMailService.SendMailToOne(email, subject, body);
        }

        public void VerifyOTP(VerifyOTPRequest verifyOTPRequest)
        {
            ThrowIfInvalidCode(verifyOTPRequest.Type, verifyOTPRequest.Email, verifyOTPRequest.Token);
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
            var user = await GetUserExistsById(userId);

            if (currentPassword.Equals(newPassword))
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
            var user = await GetUserExistsById(userId);
            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded;
        }

        private async Task ChangeEmail(User user, string newEmail)
        {
            await ThrowIfUserExists(newEmail);

            var result = await _userManager.SetEmailAsync(user, newEmail);
            var resultUserName = await _userManager.SetUserNameAsync(user, newEmail);

            if (!result.Succeeded || !resultUserName.Succeeded)
            {
                throw new Exception(ErrorMessage.ERROR);
            }
            var provider = ExternalLoginEnum.GOOGLE.ToString();

            var externalLogins = await _userManager.GetLoginsAsync(user);
            var googleLogin = externalLogins.SingleOrDefault(e => e.LoginProvider == provider);
            if (googleLogin != null)
            {
                var res = await _userManager.RemoveLoginAsync(user, provider, googleLogin.ProviderKey);
                if (!res.Succeeded)
                {
                    throw new Exception(ErrorMessage.ERROR);
                }
            }
        }

        public async Task ChangeEmail(string userId, string newEmail, string token)
        {
            using var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                var user = await GetUserExistsById(userId);
                ThrowIfInvalidCode(AuthTypeEnum.ChangeEmail, newEmail, token);

                await ChangeEmail(user, newEmail);

                RemoveCache(AuthTypeEnum.ChangeEmail, newEmail, token);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task ResetPassword(User user, string password)
        {
            var tempToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, tempToken, password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join("; ", result.Errors));
            }
        }

        public async Task ResetPassword(string email, string token, string password)
        {
            var user = await GetUserExistsByUserName(email);
            ThrowIfInvalidCode(AuthTypeEnum.ForgetPassword, email, token);

            await ResetPassword(user, password);

            RemoveCache(AuthTypeEnum.ForgetPassword, email, token);
        }


        public async Task<UserResponse> UpdateUserAccount(string userId, UpdateAccountRequest request)
        {
            var user = await GetUserExistsById(userId);

            var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
                {
                    await ChangeEmail(user, request.Email);
                }
                if (!string.IsNullOrEmpty(request.Password))
                {
                    await ResetPassword(user, request.Password);
                }
                user.Fullname = request.Fullname;
                user.PhoneNumber = request.PhoneNumber;
                
                var currentRoles = await _userManager.GetRolesAsync(user);
                var removeRole = await _userManager
                    .RemoveFromRolesAsync(user, currentRoles.Where(role => !request.Roles.Contains(role)));
                if (!removeRole.Succeeded)
                {
                    throw new Exception(ErrorMessage.ERROR);
                }
                await AddToRolesAsync(user, request.Roles.Except(currentRoles));

                await _userManager.UpdateAsync(user);
                await transaction.CommitAsync();

                var res = _mapper.Map<UserResponse>(user);
                res.LockedOut = res.LockoutEnd > DateTime.Now;
                res.LockoutEnd = res.LockoutEnd > DateTime.Now ? res.LockoutEnd : null;
                res.Roles = request.Roles.Select(e => e.ToString());

                return res;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<UserResponse> CreateUserWithRoles(AccountRequest request)
        {
            var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                await ThrowIfUserExists(request.Email);
                var user = await CreateUserAsync(request.Email, request.Password, request.Fullname, request.PhoneNumber);

                await AddToRolesAsync(user, request.Roles.Select(e => e.ToString()));
                await transaction.CommitAsync();

                var res = _mapper.Map<UserResponse>(user);
                res.LockedOut = res.LockoutEnd > DateTime.Now;
                res.LockoutEnd = res.LockoutEnd > DateTime.Now ? res.LockoutEnd : null;
                res.Roles = request.Roles.Select(e => e.ToString());
                return res;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
