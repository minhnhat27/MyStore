using System;
using System.ComponentModel.DataAnnotations;
namespace MyStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(30)]
        public required string Name { get; set; }
        [MaxLength(500)]
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public int Sold { get; set; }

        public required string Gender { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; }

        public ICollection<ProductMaterial> Materials { get; } = new HashSet<ProductMaterial>();
        public ICollection<ProductSize> Sizes { get; } = new HashSet<ProductSize>();
        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
        public ICollection<Image> Images { get; } = new HashSet<Image>();
        public ICollection<ProductReview> ProductReviews { get; } = new HashSet<ProductReview>();
    }
}
