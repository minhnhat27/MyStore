namespace MyStore.Domain.Entities
{
    public class OrderDetail
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }

        public long? ProductId { get; set; }
        public Product? Product { get; set; }

        public string ProductName { get; set; }

        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public int SizeId { get; set; }
        public int ColorId { get; set; }

        public double OriginPrice { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
    }
}
