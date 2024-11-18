namespace MyStore.Application.DTOs
{
    public class SizeDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateSizeRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
