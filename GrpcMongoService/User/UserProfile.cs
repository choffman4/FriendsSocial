﻿using MongoDB.Bson.Serialization.Attributes;
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
        public string Username { get; set; }
        public bool IsActive { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string HomeTown { get; set; }
        public string Occupation { get; set; }
        public string ExternalLink { get; set; }
        public DateTime JoinedDate { get; set; }
        public DateTime DateOfBirth { get; set; }

        public string ProfilePictureUrl { get; set; }
        public string CoverPictureUrl { get; set; }

        // Add default values or custom logic in the constructor if necessary
        public UserProfile(string userid)
        {
            UserId = userid;
            JoinedDate = DateTime.UtcNow.Date;
            IsActive = true;
        }
    }
}
