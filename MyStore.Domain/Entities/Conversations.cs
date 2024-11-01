using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyStore.Domain.Entities
{
    public class Conversations
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public IEnumerable<Message> Messages { get; set; } = new List<Message>();
    }

    public class Message
    {
        public string Content { get; set; }
        public bool IsUser { get; set; }
        public DateTime? CreateAt { get; set; } = DateTime.Now;
    }
}
