using Microsoft.EntityFrameworkCore;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(MaterialId))]
    public class ProductMaterial : IBaseEntity
    {
        public long ProductId { get; set; }
        public Product Product { get; set; }
        public int MaterialId { get; set; }
        public Material Material { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
