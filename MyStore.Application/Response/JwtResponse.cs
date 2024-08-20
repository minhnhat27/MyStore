namespace MyStore.Application.Response
{
    public class JwtResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string? FullName { get; set; }
        public string Session { get; set; }
    }
}
