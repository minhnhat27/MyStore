using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MyStore.Domain.Entities
{
    public class Notifications
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public bool Read { get; set; }
        public string Message { get; set; }
    }
}
