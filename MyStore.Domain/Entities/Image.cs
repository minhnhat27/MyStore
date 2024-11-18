using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Image : IBaseEntity
    {
        public long Id { get; set; }

        public long ProductId { get; set; }
        public Product Product { get; set; }

        [MaxLength(200)]
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
