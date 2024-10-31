namespace MyStore.Application.DTOs
{
    public class VoucherDTO
    {
        public string Code { get; set; }

        public int? DiscountPercent { get; set; }
        public double? DiscountAmount { get; set; }

        public double MinOrder { get; set; }
        public double MaxDiscount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public bool IsGlobal { get; set; }
    }
}
