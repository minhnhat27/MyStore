using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyStore.Domain.Entities
{
    public class Conversations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public List<Message> Messages { get; set; } = new();
        public Unread Unread { get; set; } = new();
        public bool Closed { get; set; }
    }
    public class Message
    {
        public string Content { get; set; }
        public bool IsUser { get; set; }
        public string? Image { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.Now;
    }
    public class Unread
    {
        public int User { get; set; }
        public int Admin { get; set; }
    }
}
