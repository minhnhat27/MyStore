using Microsoft.AspNetCore.Identity;

namespace MyStore.Domain.Entities
{
    public class Role : IdentityRole
    {
        public virtual ICollection<UserRole> UserRoles { get; }
    }
}
