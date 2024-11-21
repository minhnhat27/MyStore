namespace MyStore.Application.Response
{
    public class JwtResponse
    {
        public string AccessToken { get; set; }
        public DateTime Expires { get; set; }
        //public bool IsAdmin { get; set; } = false;
        public IEnumerable<string> Roles { get; set; }
    }

    public class LoginResponse : JwtResponse
    {
        public string? Fullname { get; set; }
        public string Session { get; set; }
    }
}
