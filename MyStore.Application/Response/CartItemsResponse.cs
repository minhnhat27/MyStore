namespace MyStore.Application.Response
{
    public class CartItemsResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public float DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
    }
}
