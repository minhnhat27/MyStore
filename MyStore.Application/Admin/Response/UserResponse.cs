using MyStore.Application.DTO;

namespace MyStore.Application.Admin.Response
{
    public class UserResponse : UserDTO
    {
        public bool EmailConfirmed { get; set; }
        public bool LockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
