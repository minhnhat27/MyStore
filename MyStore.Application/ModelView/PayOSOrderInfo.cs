namespace MyStore.Application.ModelView
{
    public class ProductInfo
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
    public class PayOSOrderInfo
    {
        public long OrderId { get; set; }
        public IEnumerable<ProductInfo> Products { get; set; }
        public double Amount { get; set; }
    }

    public class PayOSRequest
    {
        public string Code { get; set; }
        public string Id { get; set; }
        public bool Cancel { get; set; }
        public string Status { get; set; }
        public long OrderCode { get; set; }
    }
}
