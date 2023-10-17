using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace PostingService.Post
{
    public class Post
    {
        // This attribute is important as MongoDB requires an ObjectId as the primary key (_id). 
        // However, to make this easier for .NET developers, we can use this attribute to convert between a string representation and ObjectId.
        [BsonId]
        public ObjectId Id { get; set; }

        public string PostId { get; set; }
        public string UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string PrivacyType { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime LastEditedDate { get; set; }

        public List<String> UserIdLikes { get; set; }
        public List<String> ChildCommentIds { get; set; }

        public Post(string userid, string title, string content, string privacyType)
        {
            UserId = userid;
            Title = title;
            Content = content;
            PrivacyType = privacyType;    
            PostId = Guid.NewGuid().ToString();
            PostedDate = DateTime.UtcNow.Date;
            LastEditedDate = DateTime.UtcNow.Date;
            UserIdLikes = new List<String>();
            ChildCommentIds = new List<String>();
        }
    }
}
