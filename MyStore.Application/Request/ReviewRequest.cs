using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Application.Request
{
    public class ReviewRequest
    {
        public long ProductId { get; set; }

        [Range(0, 5)]
        public int Star { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }
        public IFormFileCollection? Images { get; set; }
    }
}
