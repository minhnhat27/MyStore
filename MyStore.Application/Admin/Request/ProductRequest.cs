using MyStore.Application.ModelView;

namespace MyStore.Application.Admin.Request
{
    public class ProductRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public string Gender { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public double Price { get; set; }
        public double DiscountPercent { get; set; } = 0;
        public IEnumerable<int> MaterialIds { get; set; } = new List<int>();
        public IEnumerable<SizeAndQuantity> SizesAndQuantities { get; set; } = new List<SizeAndQuantity>();
        public IEnumerable<string>? ImageUrls { get; set; } = new List<string>();
    }
}
