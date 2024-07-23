using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(SizeId))]
    public class ProductSize : IBaseEntity
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string SizeId { get; set; }
        public Size Size { get; set; }
        [Range(0, int.MaxValue)]
        public int InStock { get; set; }
        [Range(0, 100)]
        public double DiscountPercent { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
