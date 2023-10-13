using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text;

namespace GrpcMongoFriendsService.Friends
{
    public class FriendRequest
    {
        // This attribute is important as MongoDB requires an ObjectId as the primary key (_id). 
        // However, to make this easier for .NET developers, we can use this attribute to convert between a string representation and ObjectId.
        [BsonId]
        public ObjectId Id { get; set; }

        public string friendshipRequestId { get; set; }
        public string senderId { get; set; }
        public string receiverId { get; set; }

        public FriendRequest(string senderid, string receiverid)
        {
            friendshipRequestId = CombineAndSortGUIDs(senderid, receiverid);
            senderId = senderid;
            receiverId = receiverid;
        }

        private static string CombineAndSortGUIDs(string guid1, string guid2)
        {
            // Remove dashes from both GUIDs
            string cleanedGuid1 = guid1.Replace("-", "");
            string cleanedGuid2 = guid2.Replace("-", "");

            // Combine both cleaned GUIDs into a single string
            string combined = cleanedGuid1 + cleanedGuid2;

            // Convert the combined string to a character array
            char[] chars = combined.ToCharArray();

            // Sort the character array
            Array.Sort(chars);

            // Create a new string from the sorted character array
            return new string(chars);
        }

    }
}
