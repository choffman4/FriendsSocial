using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoMessagingService.MessageDocument
{
    public class MessageObject
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("senderId")]
        public string SenderId { get; set; }

        [BsonElement("receiverId")]
        public string ReceiverId { get; set; }

        [BsonElement("content")]
        public string Content { get; set; }

        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("isRead")]
        public bool IsRead { get; set; }
    }
}
