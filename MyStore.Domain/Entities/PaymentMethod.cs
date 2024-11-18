using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
