using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(CampaignId))]
    public class ProductCampaign
    {
        public long ProductId { get; set; }
        public Product Product { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }

        [Range(0, 100)]
        public int DiscountPercent { get; set; }
        public int Quantity { get; set; }
    }
}
