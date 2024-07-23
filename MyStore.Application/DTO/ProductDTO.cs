namespace MyStore.Application.DTO
{
    public class ProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public int Sold { get; set; }
        public string Gender { get; set; }
        public double Price { get; set; }
        public string CategoryName { get; set; }
        public string BrandName { get; set; }
        public string ImageUrl { get; set; }
    }
}
