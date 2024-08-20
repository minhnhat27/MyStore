namespace MyStore.Application.Request
{
    public class Filters : PageRequest
    {
        public string Sort { get; set; } = "";
        public IEnumerable<int>? Categories { get; set; }
        public IEnumerable<int>? Brands { get; set; }
        public int? Rating { get; set; }
        public double? MinPrice { get; set; }
        public double? MaxPrice { get; set; }
    }
}
