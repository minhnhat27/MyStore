namespace MyStore.Application.Request
{
    public class IdRequest<TKey>
    {
        public TKey Id { get; set; }
    }
    public class NameRequest
    {
        public string Name { get; set; }
    }
    public class EmailRequest
    {
        public string Email { get; set; }
    }
    public class PasswordRequest
    {
        public string Password { get; set; }
    }
    public class TokenRequest
    {
        public string Token { get; set; }
    }
    public class CodeRequest
    {
        public string Code { get; set; }
    }
}
