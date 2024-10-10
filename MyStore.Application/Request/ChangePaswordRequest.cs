namespace MyStore.Application.Request
{
    public class ChangePaswordRequest
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
