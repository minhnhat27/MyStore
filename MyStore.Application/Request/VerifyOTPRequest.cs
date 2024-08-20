namespace MyStore.Application.Request
{
    public class VerifyOTPRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
