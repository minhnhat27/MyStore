using MyStore.Domain.Entities;

namespace MyStore.Application.DTOs
{
    public class ConversationDTO
    {
        public string? Id { get; set; }
        public IEnumerable<Message> Messages { get; set; }
    }
}
