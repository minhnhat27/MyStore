using MyStore.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Order : IBaseEntity
    {
        public int Id { get; set; }
        [Range(0, double.MaxValue)]
        public double Total { get; set; }
        [Range(0, double.MaxValue)]
        public double ShippingCost { get; set; } 
        public DateTime OrderDate { get; set; }
        [MaxLength(100)]
        public string ShippingAddress { get; set; }
        [MaxLength(100)]
        public string ReceiverInfo { get; set; }
        public bool Paid { get; set; } = false;

        public string PaymentMethodName { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string OrderStatusName { get; set; } = DeliveryStatus.Proccessing.ToString();
        public OrderStatus OrderStatus { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
    }
}
