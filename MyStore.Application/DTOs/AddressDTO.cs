namespace MyStore.Application.DTOs
{
    public class AddressDTO
    {
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }

        public int? Province_id { get; set; }
        public string? Province_name { get; set; }

        public int? District_id { get; set; }
        public string? District_name { get; set; }

        public int? Ward_id { get; set; }
        public string? Ward_name { get; set; }

        public string? Detail { get; set; }
    }
}
