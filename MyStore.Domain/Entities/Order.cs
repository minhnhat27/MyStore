using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public double Total { get; set; }
        public double ShippingCost { get; set; } 
        public DateTime OrderDate { get; set; }
        public required string DeliveryAddress { get; set; }
        public required string ReceiverInfo { get; set; }
        public bool Paid { get; set; } = false;

        public required string PaymentMethodName { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        public required string OrderStatusName { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public required string UserId { get; set; }
        public User? User { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
    }
}
