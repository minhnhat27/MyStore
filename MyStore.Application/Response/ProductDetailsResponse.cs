using MyStore.Application.DTOs;
using MyStore.Application.ModelView;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Response
{
    public class ColorSizeResponse
    {
        public long Id { get; set; }
        public string ColorName { get; set; }
        public string ImageUrl { get; set; }
        public IEnumerable<SizeInStock> SizeInStocks { get; set; }
    }
    public class ProductDetailsResponse : ProductDTO
    {
        public string? Description { get; set; }
        public IEnumerable<ColorSizeResponse> ColorSizes { get; set; }
        public IEnumerable<string> ImageUrls { get; set; }
    }
}
