using System.ComponentModel.DataAnnotations;

namespace MyStore.Application.Request
{
    public class OrderRequest
    {
        public double Total { get; set; }
        public double ShippingCost { get; set; }
        public string? Code { get; set; }

        [MaxLength(80, ErrorMessage = "Thông tin quá dài")]
        public string Receiver { get; set; }

        [MaxLength(150, ErrorMessage = "Địa chỉ quá dài")]
        public string DeliveryAddress { get; set; }
        public IEnumerable<string> CartIds { get; set; }
        public int PaymentMethodId { get; set; }

        public string? UserIP { get; set; }
    }

    public class UpdateOrderRequest
    {
        public string? DeliveryAddress { get; set; }
        public string? ReceiverInfo { get; set; }
    }
}
