using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    //[PrimaryKey(nameof(ProductId), nameof(UserId), nameof(SizeId), nameof(ColorId))]
    public class CartItem : IBaseEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public long ProductId { get; set; }
        public Product Product { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public int Quantity { get; set; }

        public long SizeId { get; set; }
        public long ColorId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
