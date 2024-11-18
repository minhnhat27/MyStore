using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Material : IBaseEntity
    {
        public int Id { get; set; }
        [MaxLength(30)]
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ProductMaterial> Products { get; } = new HashSet<ProductMaterial>();
    }
}
