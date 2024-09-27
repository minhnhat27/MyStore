using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class User : IdentityUser, IBaseEntity
    {
        [MaxLength(50)]
        public string? Fullname { get; set; }

        public DeliveryAddress? DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Order> Orders { get; } = new HashSet<Order>();
        public ICollection<UserVoucher> UserVouchers { get; } = new HashSet<UserVoucher>();
    }
}
