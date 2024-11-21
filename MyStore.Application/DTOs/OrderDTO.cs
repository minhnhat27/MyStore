using MyStore.Domain.Enumerations;

namespace MyStore.Application.DTOs
{
    public class OrderDTO
    {
        public long Id { get; set; }
        public double ShippingCost { get; set; }
        public double Total { get; set; }
        public double AmountPaid { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? ReviewDeadline { get; set; }
        public bool Reviewed { get; set; }

        public string PaymentMethodName { get; set; }
        public DeliveryStatusEnum OrderStatus { get; set; }
        public double VoucherDiscount { get; set; }

        public string DeliveryAddress { get; set; }
        public string Receiver { get; set; }

        public string? ShippingCode { get; set; }
        public DateTime? Expected_delivery_time { get; set; }
    }

    public class OrderResponse : OrderDTO
    {
        public ProductOrderDetail ProductOrderDetail { get; set; }
        //public string? PayBackUrl { get; set; }
        public DateTime? PaymentDeadline { get; set; }
    }

    public class OrderDetailsResponse : OrderDTO
    {
        public IEnumerable<ProductOrderDetail> ProductOrderDetails { get; set; }
 
    }

    public class ProductOrderDetail
    {
        public long? ProductId { get; set; }
        public string ProductName { get; set; }
        public string Variant { get; set; }
        public double OriginPrice { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public string? ImageUrl { get; set; }

    }
}
