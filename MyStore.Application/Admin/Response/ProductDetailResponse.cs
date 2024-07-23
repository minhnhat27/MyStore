using MyStore.Application.ModelView;

namespace MyStore.Application.Admin.Response
{
    public class ProductDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public string Gender { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public double Price { get; set; }
        public IEnumerable<int> MaterialIds { get; set; }
        public IEnumerable<string> SizeIds { get; set; }
        public IEnumerable<SizeAndQuantity> SizesAndQuantities { get; set; }
        public IEnumerable<string> ImageUrls { get; set; }
    }
}
