using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class ProductFeature : IBaseEntity
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; }

        public int Label { get; set; }
        public double Green { get; set; }
        public double Red { get; set; }
        public double Blue { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
