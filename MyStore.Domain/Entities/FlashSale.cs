using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class FlashSale
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public ICollection<Product> Products { get; set; } = new HashSet<Product>();
    }
}
