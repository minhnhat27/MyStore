namespace MyStore.Application.Request
{
    public class ResetPasswordRequest
    {
        public string Password { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
