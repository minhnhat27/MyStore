namespace MyStore.Application.Request
{
    public class UpdateOrderRequest
    {
        public string? ShippingAddress { get; set; }
        public string? ReceiverInfo { get; set; }
    }
}
