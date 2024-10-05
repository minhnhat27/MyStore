namespace MyStore.Application.Request
{
    public class SendCodeRequest
    {
        public string Email { get; set; }
    }
    public class VerifyOTPRequest : SendCodeRequest
    {
        public string Token { get; set; }
    }
}
