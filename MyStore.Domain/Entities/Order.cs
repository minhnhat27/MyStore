using MyStore.Domain.Enumerations;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class Order : IBaseEntity
    {
        public long Id { get; set; }

        [Range(0, double.MaxValue)]
        public double Total { get; set; }

        [Range(0, double.MaxValue)]
        public double ShippingCost { get; set; }

        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public DateTime? ReviewDeadline { get; set; }

        public bool Reviewed { get; set; }
        public double VoucherDiscount { get; set; }

        [MaxLength(160)]
        public string DeliveryAddress { get; set; }

        [MaxLength(100)]
        public string Receiver { get; set; }

        public string WardID { get; set; } //GHN -> WardCode
        public int DistrictID { get; set; }

        public string? ShippingCode { get; set; }
        public DateTime? Expected_delivery_time { get; set; }

        public double AmountPaid { get; set; }

        public string? PaymentTranId { get; set; }

        public int? PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }

        [MaxLength(30)]
        public string PaymentMethodName { get; set; }

        public DeliveryStatusEnum? OrderStatus { get; set; } = DeliveryStatusEnum.Processing;

        public string? UserId { get; set; }
        public User? User { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; } = new HashSet<OrderDetail>();
    }
}
