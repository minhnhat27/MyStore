namespace MyStore.Application.Response
{
    public class JwtResponse
    {
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        //public string RefreshToken { get; set; }
        public string? Fullname { get; set; }
        public string Session { get; set; }
    }
}
