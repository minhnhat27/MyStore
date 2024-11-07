using MyStore.Domain.Enumerations;

namespace MyStore.Application.DTOs
{
    public class FlashSaleDTO
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public DiscountTimeFrame DiscountTimeFrame { get; set; }
        public int ProductQuantity { get; set; }
    }
}
