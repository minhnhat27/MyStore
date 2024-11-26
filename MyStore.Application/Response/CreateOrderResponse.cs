namespace MyStore.Application.Response
{
    public class CreateOrderResponse
    {
        public long Id { get; set; }
        public string? PaymentUrl { get; set; }
    }
}
