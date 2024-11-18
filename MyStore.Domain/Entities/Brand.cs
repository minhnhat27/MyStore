using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Brand : IBaseEntity
    {
        public int Id { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get; } = new HashSet<Product>();
    }
}
