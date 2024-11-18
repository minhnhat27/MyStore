using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class ProductColor : IBaseEntity
    {
        public long Id { get; set; }
        [MaxLength(20)]
        public string ColorName { get; set; }
        
        public long ProductId { get; set; }
        public Product Product { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<ProductSize> ProductSizes { get; } = new HashSet<ProductSize>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
