using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoService.User
{
    public class UserProfile
    {
        // This attribute is important as MongoDB requires an ObjectId as the primary key (_id). 
        // However, to make this easier for .NET developers, we can use this attribute to convert between a string representation and ObjectId.
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; }

        // Other properties for the user profile can go here, for example:
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        // ... any other necessary fields ...

        // Add default values or custom logic in the constructor if necessary
        public UserProfile()
        {
            // Default values or logic on creation can go here
        }
    }
}
