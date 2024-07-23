using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class User : IdentityUser, IBaseEntity
    {
        [MaxLength(50)]
        public string? Fullname { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<Address> Addresses { get; } = new HashSet<Address>();
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
