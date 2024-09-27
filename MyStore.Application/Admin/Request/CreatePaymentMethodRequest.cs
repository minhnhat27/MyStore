namespace MyStore.Application.Admin.Request
{
    public class CreatePaymentMethodRequest
    {
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
    }
}
