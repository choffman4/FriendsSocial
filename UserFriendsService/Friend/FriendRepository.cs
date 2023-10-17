using Grpc.Core;
using GrpcMongoFriendService;

namespace UserFriendsService.Friend
{
    public class FriendRepository : IFriendRepository
    {
        private readonly MongoFriendService.MongoFriendServiceClient _grpcClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FriendRepository> _logger;

        public FriendRepository(ILogger<FriendRepository> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            var host = _configuration["GrpcService:Host"];
            var port = _configuration["GrpcService:Port"];
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _grpcClient = new MongoFriendService.MongoFriendServiceClient(channel);
            _logger = logger;
        }

        public async Task<AcceptFriendRequestResponse> AcceptFriendRequestAsync(AcceptFriendRequestRequest request)
        {
            return await _grpcClient.AcceptFriendRequestAsync(new AcceptFriendRequestRequest { FriendshipId = request.FriendshipId });
        }

        public async IAsyncEnumerable<GetFriendRequestsReceivedResponse> GetFriendRequestsReceivedAsync(GetFriendRequestsReceivedRequest request)
        {
            var responseStream = _grpcClient.GetFriendRequestsReceived(request);

            await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
            {
                yield return response;
            }
        }

        public async IAsyncEnumerable<GetFriendRequestsSentResponse> GetFriendRequestsSentAsync(GetFriendRequestsSentRequest request)
        {
            var responseStream = _grpcClient.GetFriendRequestsSent(request);

            await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
            {
                yield return response;
            }
        }

        public async IAsyncEnumerable<GetFriendsResponse> GetFriendsAsync(GetFriendsRequest request)
        {
            var responseStream = _grpcClient.GetFriends(request);

            await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
            {
                yield return response;
            }
        }

        public async Task<GetFriendshipResponse> GetFriendshipAsync(GetFriendshipRequest request)
        {
            try
            {
                var response = await _grpcClient.GetFriendshipAsync(request);

                var result = new GetFriendshipResponse
                {
                    Message = response.Message
                };

                return result;
            } catch (RpcException ex)
            {
                throw new Exception("Error occurred while invoking GetFriendship RPC.", ex);
            }
        }

        public async Task<RemoveFriendResponse> RemoveFriendAsync(RemoveFriendRequest request)
        {
            return await _grpcClient.RemoveFriendAsync(request);
        }

        public async Task<SendFriendRequestResponse> SendFriendRequestAsync(SendFriendRequestRequest request)
        {
            return await _grpcClient.SendFriendRequestAsync(request);
        }
    }

}
