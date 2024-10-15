using MyStore.Domain.Entities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Application.DTOs
{
    public class ReviewDTO
    {
        public string Id { get; set; }
        public string? Description { get; set; }
        public int Star { get; set; }
        public string Username { get; set; }
        public List<string>? ImagesUrls { get; set; } = new List<string>();
    }
}
