namespace MyStore.Domain.Entities
{
    public class Brand : IBaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get; } = new HashSet<Product>();
    }
}
