using MyStore.Application.DTOs;

namespace MyStore.Application.Response
{
    public class UserResponse : UserDTO
    {
        public string Id { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
