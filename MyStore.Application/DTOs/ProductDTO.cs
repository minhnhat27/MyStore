using Microsoft.AspNetCore.Http;
using MyStore.Application.ModelView;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.DTOs
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public int Sold { get; set; }
        public string Gender { get; set; }
        public double Price { get; set; }
        public float DiscountPercent { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public float Rating { get; set; }
        public long RatingCount { get; set; }
        public string ImageUrl { get; set; }
    }
    public class ColorSizeRequest
    {
        public string ColorName { get; set; }
        public IFormFile? Image { get; set; }
        public IEnumerable<SizeInStock> SizeInStocks { get; set; }

        //update
        public long? Id { get; set; }
        public string? ImageUrl { get; set; }
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
        public IEnumerable<ColorSizeRequest> ColorSizes { get; set; }

        //for update
        public IEnumerable<string>? ImageUrls { get; set; }
    }
    public class UpdateEnableRequest
    {
        public bool Enable { get; set; }
    }
}
