using Grpc.Core;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using GrpcMongoFriendsService.Friends;

namespace GrpcMongoFriendsService.Services
{
    public class FriendGrpcService : MongoFriendService.MongoFriendServiceBase
    {
        private readonly ILogger<FriendGrpcService> _logger;
        private readonly IConfiguration _configuration;

        public FriendGrpcService(ILogger<FriendGrpcService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        // Implement the SendFriendRequest RPC method
        public override Task<SendFriendRequestResponse> SendFriendRequest(SendFriendRequestRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friend requests
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendRequestsCollection = database.GetCollection<FriendRequest>("friendRequests");

                // Create a new friend request document
                var friendRequest = new FriendRequest(request.Sender, request.Receiver);

                // Insert the friend request document into the collection
                friendRequestsCollection.InsertOne(friendRequest);

                return Task.FromResult(new SendFriendRequestResponse { Message = "Friend request sent successfully." });
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending a friend request.");
                return Task.FromResult(new SendFriendRequestResponse { Message = "An error occurred while sending the friend request." });
            }
        }


        // Implement the AcceptFriendRequest RPC method
        public override Task<AcceptFriendRequestResponse> AcceptFriendRequest(AcceptFriendRequestRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friend requests
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendRequestsCollection = database.GetCollection<FriendRequest>("friendRequests");
                var friendshipsCollection = database.GetCollection<Friendship>("friendships");

                // Find the friend request by its ID
                var filter = Builders<FriendRequest>.Filter.Eq(fr => fr.friendshipRequestId, request.FriendshipId);
                var friendRequest = friendRequestsCollection.Find(filter).FirstOrDefault();

                if (friendRequest == null)
                {
                    return Task.FromResult(new AcceptFriendRequestResponse { Message = "Friend request not found." });
                }

                // Create a Friendship document
                var friendship = new Friendship(request.FriendshipId, friendRequest.senderId, friendRequest.receiverId);

                // Insert the Friendship document into the collection
                friendshipsCollection.InsertOne(friendship);

                // Delete the friend request after it's accepted
                friendRequestsCollection.DeleteOne(filter);

                return Task.FromResult(new AcceptFriendRequestResponse { Message = "Friend request accepted successfully." });
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while accepting a friend request.");
                return Task.FromResult(new AcceptFriendRequestResponse { Message = "An error occurred while accepting the friend request." });
            }
        }


        // Implement the GetFriendRequestsSent RPC method
        public override async Task GetFriendRequestsSent(GetFriendRequestsSentRequest request, IServerStreamWriter<GetFriendRequestsSentResponse> responseStream, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friend requests
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendRequestsCollection = database.GetCollection<FriendRequest>("friendRequests");

                // Find friend requests sent by the user
                var filter = Builders<FriendRequest>.Filter.Eq(fr => fr.senderId, request.UserId);
                var friendRequests = await friendRequestsCollection.Find(filter).ToListAsync();

                foreach (var friendRequest in friendRequests)
                {
                    // Convert the FriendRequest to a GetFriendRequestsSentResponse message
                    var response = new GetFriendRequestsSentResponse
                    {
                        FriendRequests = new FriendRequestMessage
                        {
                            Sender = friendRequest.senderId,
                            Receiver = friendRequest.receiverId
                        }
                    };

                    // Stream the response
                    await responseStream.WriteAsync(response);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting friend requests sent by a user.");
            }
        }


        // Implement the GetFriendRequestsReceived RPC method
        public override async Task GetFriendRequestsReceived(GetFriendRequestsReceivedRequest request, IServerStreamWriter<GetFriendRequestsReceivedResponse> responseStream, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friend requests
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendRequestsCollection = database.GetCollection<FriendRequest>("friendRequests");

                // Find friend requests received by the user
                var filter = Builders<FriendRequest>.Filter.Eq(fr => fr.receiverId, request.UserId);
                var friendRequests = await friendRequestsCollection.Find(filter).ToListAsync();

                foreach (var friendRequest in friendRequests)
                {
                    // Convert the FriendRequest to a GetFriendRequestsReceivedResponse message
                    var response = new GetFriendRequestsReceivedResponse
                    {
                        FriendRequests = new FriendRequestMessage
                        {
                            Sender = friendRequest.senderId,
                            Receiver = friendRequest.receiverId
                        }
                    };

                    // Stream the response
                    await responseStream.WriteAsync(response);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting friend requests received by a user.");
            }
        }


        // Implement the GetFriends RPC method
        public override async Task GetFriends(GetFriendsRequest request, IServerStreamWriter<GetFriendsResponse> responseStream, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friendships
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendshipsCollection = database.GetCollection<Friendship>("friendships");

                // Find friendships for the user
                var filter = Builders<Friendship>.Filter.Eq(f => f.friend1Id, request.UserId) | Builders<Friendship>.Filter.Eq(f => f.friend2Id, request.UserId);
                var friendships = await friendshipsCollection.Find(filter).ToListAsync();

                foreach (var friendship in friendships)
                {
                    // Determine the friend's ID based on the current user's ID
                    var friendId = request.UserId == friendship.friend1Id ? friendship.friend2Id : friendship.friend1Id;

                    // Create a GetFriendsResponse message for each friend
                    var response = new GetFriendsResponse
                    {
                        FriendId = friendId
                    };

                    // Stream the response
                    await responseStream.WriteAsync(response);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting a list of friends for a user.");
            }
        }


        // Implement the RemoveFriend RPC method
        public override Task<RemoveFriendResponse> RemoveFriend(RemoveFriendRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection for friendships
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileFriends");
                var friendshipsCollection = database.GetCollection<Friendship>("friendships");

                // Find the friendship document based on the provided friendshipId
                var filter = Builders<Friendship>.Filter.Eq(f => f.friendshipId, request.FriendshipId);
                var friendship = friendshipsCollection.Find(filter).FirstOrDefault();

                if (friendship != null)
                {
                    // Delete the friendship document
                    friendshipsCollection.DeleteOne(filter);

                    return Task.FromResult(new RemoveFriendResponse { Message = "Friend removed successfully." });
                } else
                {
                    return Task.FromResult(new RemoveFriendResponse { Message = "Friendship not found or failed to remove." });
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while removing a friend.");
                return Task.FromResult(new RemoveFriendResponse { Message = "An error occurred while removing the friend." });
            }
        }

    }
}
