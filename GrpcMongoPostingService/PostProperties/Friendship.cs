using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoPostingService.PostProperties
{
    public class Friendship
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string friendshipId { get; set; }
        public string friend1Id { get; set; }
        public string friend2Id { get; set; }
    }
}
