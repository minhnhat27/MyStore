using MyStore.Application.Model;

namespace MyStore.Application.Admin.Request
{
    public class CreateProductRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public required string Gender { get; set; }
        public int Category { get; set; }
        public int Brand { get; set; }
        public required List<int> Materials { get; set; }
        public required IEnumerable<SizeAndQuantity> Sizes { get; set; }
    }
}
