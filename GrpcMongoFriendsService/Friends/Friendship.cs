using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoFriendsService.Friends
{
    public class Friendship
    {
        // This attribute is important as MongoDB requires an ObjectId as the primary key (_id). 
        // However, to make this easier for .NET developers, we can use this attribute to convert between a string representation and ObjectId.
        [BsonId]
        public ObjectId Id { get; set; }

        public string friendshipId { get; set; }
        public string friend1Id { get; set; }
        public string friend2Id { get; set; }

        public Friendship(string friendshipid, string friend1, string friend2)
        {
            friendshipId = friendshipid;
            friend1Id = friend1;
            friend2Id = friend2;
        }
    }
}
