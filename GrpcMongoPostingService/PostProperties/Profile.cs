using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoPostingService.PostProperties
{
    public class Profile
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Username { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
