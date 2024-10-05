using Microsoft.AspNetCore.Http;

namespace MyStore.Application.DTOs
{
    public class BrandDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }
    public class CreateBrandRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
    }
}
