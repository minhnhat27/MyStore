using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class PaymentMethod
    {
        [Key]
        public string Name { get; set; }
        public bool isActive { get; set; } = false;
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
