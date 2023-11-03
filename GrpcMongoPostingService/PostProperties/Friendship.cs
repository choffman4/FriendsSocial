using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoPostingService.PostProperties
{
    public class Friendship
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Friend1Id { get; set; }
        public string Friend2Id { get; set; }
    }
}
