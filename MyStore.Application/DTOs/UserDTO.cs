namespace MyStore.Application.DTOs
{
    public class UserInfo
    {
        public bool Facebook { get; set; }
        public string? Fullname { get; set; }
        public string? PhoneNumber { get; set; }
    }
    public class UserDTO : UserInfo
    {
        public string? Email { get; set; }
    }
}
