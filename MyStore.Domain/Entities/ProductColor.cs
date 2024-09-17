namespace MyStore.Domain.Entities
{
    public class ProductColor : IBaseEntity
    {
        public int Id { get; set; }
        public string ColorName { get; set; }
        
        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string ImageUrl { get; set; }

        public ICollection<ProductSize> ProductSizes { get; } = new HashSet<ProductSize>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
