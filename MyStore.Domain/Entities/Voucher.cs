using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Voucher
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public string DiscountPercent { get; set; }
        public string MinOrder { get; set; }
        public string MaxDiscount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
