using MyStore.Application.ModelView;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Admin.Response
{
    public class ColorSizeResponse
    {
        public long Id { get; set; }
        public string ColorName { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<SizeInStock> SizeInStocks { get; set; }
    }
    public class ProductDetailsResponse
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Sold { get; set; }
        public bool Enable { get; set; }
        public GenderEnum Gender { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public double Price { get; set; }
        public float DiscountPercent { get; set; }
        public IEnumerable<int> MaterialIds { get; set; }
        public IEnumerable<ColorSizeResponse> ColorSizes { get; set; }
        public IEnumerable<string> ImageUrls { get; set; }
    }
}
