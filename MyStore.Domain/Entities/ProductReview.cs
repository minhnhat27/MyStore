using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyStore.Domain.Entities
{
    public class ProductReview : IBaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(200)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int Star { get; set; }

        public string Variant { get; set; }

        public long? ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public User? User { get; set; }

        [NotMapped]
        public List<string>? ImagesUrls { get; set; }

        [Column(TypeName = "jsonb")]
        public string? ImagesUrlsJson
        {
            get => ImagesUrls == null ? null : JsonConvert.SerializeObject(ImagesUrls);
            set => ImagesUrls = string.IsNullOrEmpty(value) ? null : JsonConvert.DeserializeObject<List<string>>(value);
        }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
