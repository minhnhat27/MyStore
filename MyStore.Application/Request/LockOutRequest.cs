namespace MyStore.Application.Request
{
    public class LockOutRequest
    {
        public string UserId { get; set; }
        public DateTimeOffset? EndDate { get; set; }
    }
}
