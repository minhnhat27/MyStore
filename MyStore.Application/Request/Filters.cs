using MyStore.Domain.Enumerations;

namespace MyStore.Application.Request
{
    public class Filters : PageRequest
    {
        public SortEnum Sorter { get; set; } = 0;
        public IEnumerable<int>? MaterialIds { get; set; }
        public IEnumerable<int>? CategoryIds { get; set; }
        public IEnumerable<int>? BrandIds { get; set; }
        public int? Rating { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
        public GenderEnum? Gender { get; set; }
        public bool? Discount { get; set; }
        public bool? FlashSale { get; set; }
    }
}
