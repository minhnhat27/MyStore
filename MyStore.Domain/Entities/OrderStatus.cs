using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class OrderStatus
    {
        [Key]
        public string Name { get; set; }
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
