using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public double Price { get; set; }
        public bool Enable { get; set; } = true;
        public required string Size { get; set; }
        public required string InStock { get; set; }


        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public int BrandId { get; set; }
        public Brand? Brand { get; set; }

        public int MaterialId { get; set; }
        public Material? Material { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
        public ICollection<Image> Images { get; } = new HashSet<Image>();
        public ICollection<ProductReview> ProductReviews { get; } = new HashSet<ProductReview>();
    }
}
