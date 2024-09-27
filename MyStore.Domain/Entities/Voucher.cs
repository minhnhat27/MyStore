using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Voucher
    {
        [Key]
        public string Code { get; set; }

        public int? DiscountPercent { get; set; }
        public double? DiscountAmount { get; set; }

        public double MinOrder { get; set; }
        public double? MaxDiscount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<UserVoucher> UserVouchers { get; } = new HashSet<UserVoucher>();
    }
}
