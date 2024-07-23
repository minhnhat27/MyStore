using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class ProductReview : IBaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Review { get; set; }
        public int Star { get; set; } = 0;

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
