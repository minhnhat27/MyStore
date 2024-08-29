using Microsoft.AspNetCore.Http;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Admin.Request
{
    public class SizeInStock
    {
        public int SizeId { get; set; }
        public int InStock { get; set; }
    }
    public class ColorSize
    {
        public string Color { get; set; }
        public IFormFile Image { get; set; }
        public IEnumerable<SizeInStock> SizeInStocks { get; set; }
    }
    public class ProductRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; } = true;
        public GenderEnum Gender { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public double Price { get; set; }
        public float DiscountPercent { get; set; } = 0;
        public IEnumerable<int> MaterialIds { get; set; }
        public IEnumerable<ColorSize> ColorSizes { get; set; }
        public IEnumerable<string>? ImageUrls { get; set; }
    }
}
