using MyStore.Application.DTOs;

namespace MyStore.Application.Response
{
    public class OrderDetailsResponse
    {
        public long Id { get; set; }
        public IEnumerable<ProductOrderDetails> ProductOrderDetails { get; set; }
        public double Total { get; set; }
        public double ShippingCost { get; set; }
        public DateTime OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string Receiver { get; set; }
        public double AmountPaid { get; set; }
        //public string PaymentMethod { get; set; }
        //public string OrderStatus { get; set; }
    }
}
