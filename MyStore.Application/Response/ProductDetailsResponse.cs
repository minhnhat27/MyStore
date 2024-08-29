using MyStore.Application.DTO;
using MyStore.Application.ModelView;

namespace MyStore.Application.Response
{
    public class ProductDetailsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string Gender { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public double OriginalPrice { get; set; }
        public float DiscountPercent { get; set; }
        public double Price => DiscountPercent > 0 ? OriginalPrice - (OriginalPrice * (DiscountPercent / 100.0)) : OriginalPrice;
        public float Rating { get; set; }
        public IEnumerable<MaterialDTO> MaterialIds { get; set; }
        public IEnumerable<SizeAndQuantity> SizesAndQuantities { get; set; }
        public IEnumerable<string> ImageUrls { get; set; }
    }
}
