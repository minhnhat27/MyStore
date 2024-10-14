using MyStore.Domain.Enumerations;

namespace MyStore.Application.DTOs
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public double ShippingCost { get; set; }
        public double Total { get; set; }
        public double AmountPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool Reviewed { get; set; }

        public string PaymentMethod { get; set; }
        public DeliveryStatusEnum OrderStatus { get; set; }

        public string? PayBackUrl { get; set; }
    }

    public class ProductOrderDetails
    {
        public long? ProductId { get; set; }
        public string ProductName { get; set; }

        public string SizeName { get; set; }
        public string ColorName { get; set; }

        public double OriginPrice { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

    }
}
