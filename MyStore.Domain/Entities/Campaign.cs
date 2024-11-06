using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Campaign
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [MaxLength(55)]
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public double TotalRevenue { get; set; }
        public int Sold { get; set; }

        public ICollection<ProductCampaign> ProductCampaigns { get; } = new HashSet<ProductCampaign>();
    }
}
