using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Product : IBaseEntity
    {
        public int Id { get; set; }
        [MaxLength(60)]
        public string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool Enable { get; set; }
        [Range(0, int.MaxValue)]
        public int Sold { get; set; }
        [MaxLength(10)]
        public string Gender { get; set; }

        [Range(1000, double.MaxValue)]
        public double Price { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ProductMaterial> Materials { get; } = new HashSet<ProductMaterial>();
        public ICollection<ProductSize> Sizes { get; } = new HashSet<ProductSize>();
        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
        public ICollection<Image> Images { get; } = new HashSet<Image>();
        public ICollection<ProductReview> ProductReviews { get; } = new HashSet<ProductReview>();
    }
}
