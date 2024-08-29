using MyStore.Application.ModelView;

namespace MyStore.Application.Response
{
    public class OrderDetailsResponse
    {
        public int Id { get; set; }
        public IEnumerable<ProductsOrderDetail> Products { get; set; }
        public double SubTotal { get; set; }
        public double ShippingCost { get; set; }
        public double Total { get; set; }
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public string ReceiverInfo { get; set; }
        public string PaymentMethodName { get; set; }
        public string OrderStatusName { get; set; }
    }
}
