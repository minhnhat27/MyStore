using MyStore.Domain.Enumerations;

namespace MyStore.Application.Request
{
    public class SendCodeRequest
    {
        public string Email { get; set; }
        public AuthTypeEnum Type { get; set; } = AuthTypeEnum.Register;
    }
    public class VerifyOTPRequest : SendCodeRequest
    {
        public string Token { get; set; }
    }
}
