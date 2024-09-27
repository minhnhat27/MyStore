namespace MyStore.Application.Request
{
    public class UpdateCartItemRequest
    {
        public long? SizeId { get; set; }
        public int? Quantity { get; set; }
    }
}
