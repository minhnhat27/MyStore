namespace MyStore.Application.DTOs
{
    public class AddressDTO
    {
        public string Name { get; set; }
        public string? PhoneNumber { get; set; }

        public int? ProvinceID { get; set; }
        public string? ProvinceName { get; set; }

        public int? DistrictID { get; set; }
        public string? DistrictName { get; set; }

        public int? WardID { get; set; }
        public string? WardName { get; set; }

        public string? Detail { get; set; }
    }
}
