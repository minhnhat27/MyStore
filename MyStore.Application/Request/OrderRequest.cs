using MyStore.Application.ModelView;

namespace MyStore.Application.Request
{
    public class OrderRequest
    {
        public double ShippingCost { get; set; }
        public string DeliveryAddress { get; set; }
        public string ReceiverInfo { get; set; }
        public bool Paid { get; set; } = false;
        public string PaymentMethodName { get; set; }
        public IEnumerable<ProductAndQuantity> ProductsAndQuantities { get; set; }
    }
}
