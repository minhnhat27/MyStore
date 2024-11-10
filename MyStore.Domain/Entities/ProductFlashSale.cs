using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(FlashSaleId))]
    public class ProductFlashSale
    {
        public long ProductId { get; set; }
        public Product Product { get; set; }
        public string FlashSaleId { get; set; }
        public FlashSale FlashSale { get; set; }

        [Range(0, 100)]
        public float DiscountPercent { get; set; }
    }
}
