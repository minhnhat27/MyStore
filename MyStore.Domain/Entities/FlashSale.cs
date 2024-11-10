using MyStore.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class FlashSale : IBaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime Date { get; set; }
        public DiscountTimeFrame DiscountTimeFrame { get; set; }

        public int TotalSold { get; set; }
        public double TotalRevenue { get; set; }

        public ICollection<ProductFlashSale> ProductFlashSales { get; } = new HashSet<ProductFlashSale>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
