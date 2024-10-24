namespace MyStore.Application.DTOs
{
    public class ReviewDTO
    {
        public string Id { get; set; }
        public string? Description { get; set; }
        public int Star { get; set; }
        public string Username { get; set; }
        public string Variant { get; set; }
        public List<string>? ImagesUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
    }
}
