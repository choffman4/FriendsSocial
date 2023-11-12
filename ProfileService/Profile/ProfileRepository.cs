using Confluent.Kafka;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcMongoProfileService;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ProfileService.Profile
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly MongoProfileService.MongoProfileServiceClient _grpcClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProfileRepository> _logger;

        public ProfileRepository(ILogger<ProfileRepository> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            var host = _configuration["GrpcService:Host"];
            var port = _configuration["GrpcService:Port"];
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _grpcClient = new MongoProfileService.MongoProfileServiceClient(channel);
            _logger = logger;
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request)
        {
            return await _grpcClient.UpdateProfileAsync(request);
        }

        public async Task<GetProfileByGuidResponse> GetProfileByGuidAsync(GetProfileByGuidRequest request)
        {
            return await _grpcClient.GetProfileByGuidAsync(request);
        }

        public async Task<UsernameAvailabilityResponse> CheckUsernameAvailabilityAsync(UsernameAvailabilityRequest request)
        {
            return await _grpcClient.CheckUsernameAvailabilityAsync(request);
        }

        public async Task<GetProfileByUsernameResponse> GetProfileByUsernameAsync(GetProfileByUsernameRequest request)
        {
            return await _grpcClient.GetProfileByUsernameAsync(request);
        }

        public async IAsyncEnumerable<GetProfileSearchResponse> GetProfileSearchAsync(GetProfileSearchRequest request)
        {
            var responseStream = _grpcClient.GetProfileSearch(request);

            await foreach (var response in responseStream.ResponseStream.ReadAllAsync())
            {
                yield return response;
            }
        }
    }
}
