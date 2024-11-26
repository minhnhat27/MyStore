using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MyStore.Domain.Entities
{
    public class Notifications(string message)
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public bool IsRead { get; set; } = false;
        public string Message { get; set; } = message;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
