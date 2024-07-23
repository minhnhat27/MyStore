using Microsoft.EntityFrameworkCore;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(OrderId), nameof(ProductId))]
    public class OrderDetail
    {
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public string Size { get; set; }
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
    }
}
