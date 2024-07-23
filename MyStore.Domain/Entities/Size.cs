using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Size : IBaseEntity
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(15)]
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        //public ICollection<ProductSize> Products { get; } = new HashSet<ProductSize>();
    }
}
