using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GrpcMongoPostingService.PostProperties
{
    public class Comment
    {
        // This attribute is important as MongoDB requires an ObjectId as the primary key (_id). 
        // However, to make this easier for .NET developers, we can use this attribute to convert between a string representation and ObjectId.
        [BsonId]
        public ObjectId Id { get; set; }

        public string ParentPostId { get; set; }
        public string ParentCommentId { get; set; }
        public string CommentId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public List<String> UserIdLikes { get; set; }
        public List<String> ChildCommentIds { get; set; }
        public DateTime CommentedDate { get; set; }
        public DateTime LastEditedDate { get; set; }

        public Comment(string parentPostId, string parentCommentId, string userid, string content)
        {
            ParentPostId = parentPostId;
            ParentCommentId = parentCommentId;
            CommentId = Guid.NewGuid().ToString();
            UserId = userid;
            Content = content;
            CommentedDate = DateTime.UtcNow;
            LastEditedDate = DateTime.UtcNow;
            UserIdLikes = new List<String>();
            ChildCommentIds = new List<String>();
        }
    }
}
