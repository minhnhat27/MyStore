namespace MyStore.Application.DTOs
{
    public class UserInfo
    {
        public string? Facebook { get; set; }
        public string? Fullname { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class UserDTO : UserInfo
    {
        public string? Email { get; set; }
    }

    public class UserResponse : UserDTO
    {
        public string Id { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public IEnumerable<string> Roles { get; set; }
    }

    public class UserHaveVoucher
    {
        public IEnumerable<string> HaveVoucher { get; set; }
    }

    public class UserVoucherResponse : UserHaveVoucher
    {
        public IEnumerable<UserResponse> UserVoucher { get; set; }
    }
}
