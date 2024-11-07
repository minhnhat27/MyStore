using MyStore.Domain.Enumerations;

namespace MyStore.Application.Request
{
    public class FlashSaleRequest
    {
        public DateTime Date { get; set; }
        public DiscountTimeFrame DiscountTimeFrame { get; set; }
        public IEnumerable<ProductDiscountPercent> ProductFlashSales { get; set; }
    }
    public class ProductDiscountPercent
    {
        public long ProductId { get; set; }
        public int DiscountPercent { get; set; }
    }
}
