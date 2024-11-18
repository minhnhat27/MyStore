using MyStore.Application.DTOs;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Request
{
    public class AccountRequest : UserInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
    public class UpdateAccountRequest : UserInfo
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
