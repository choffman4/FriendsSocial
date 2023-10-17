using GrpcMongoFriendService;

namespace UserFriendsService.Friend
{
    public interface IFriendRepository
    {
        Task<SendFriendRequestResponse> SendFriendRequestAsync(SendFriendRequestRequest request);
        Task<AcceptFriendRequestResponse> AcceptFriendRequestAsync(AcceptFriendRequestRequest request);
        IAsyncEnumerable<GetFriendRequestsSentResponse> GetFriendRequestsSentAsync(GetFriendRequestsSentRequest request);
        IAsyncEnumerable<GetFriendRequestsReceivedResponse> GetFriendRequestsReceivedAsync(GetFriendRequestsReceivedRequest request);
        IAsyncEnumerable<GetFriendsResponse> GetFriendsAsync(GetFriendsRequest request);
        Task<RemoveFriendResponse> RemoveFriendAsync(RemoveFriendRequest request);
        Task<GetFriendshipResponse> GetFriendshipAsync(GetFriendshipRequest request);
    }
}
