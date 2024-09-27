namespace MyStore.Application.Admin.Request
{
    public class UpdatePaymentMethodRequest
    {
        public string? Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
