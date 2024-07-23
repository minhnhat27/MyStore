using Microsoft.AspNetCore.Http;

namespace MyStore.Application.Admin.Request
{
    public class CreateBrandRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
    }
}
