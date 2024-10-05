namespace MyStore.Application.Request
{
    public class CartRequest
    {
        public long ProductId { get; set; }
        public long SizeId { get; set; }
        public long ColorId { get; set; }
        public int Quantity { get; set; }
    }
    public class UpdateCartItemRequest
    {
        public long? SizeId { get; set; }
        public int? Quantity { get; set; }
    }
}
