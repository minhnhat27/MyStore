using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Size : IBaseEntity
    {
        public long Id { get; set; }

        [MaxLength(30)]
        public string Name { get; set; }

        public ICollection<ProductSize> ProductSizes { get; } = new HashSet<ProductSize>();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
