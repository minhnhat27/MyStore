namespace MyStore.Application.DTOs
{
    public class PaymentMethodDTO : CreatePaymentMethodRequest
    {
        public int Id { get; set; }
    }
    public class CreatePaymentMethodRequest
    {
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
    }
    public class UpdatePaymentMethodRequest
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
