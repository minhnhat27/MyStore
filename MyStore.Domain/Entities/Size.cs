using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Size : IBaseEntity
    {
        public int Id { get; set; }
        [MaxLength(15)]
        public required string Name { get; set; }
        [MaxLength(105)]
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ICollection<ProductSize> Products { get; } = new HashSet<ProductSize>();
    }
}
