using Grpc.Core;
using GrpcMongoService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace GrpcMongoService.Services
{
    public class ProfileGrpcService : ProfileService.ProfileServiceBase
    {
        private readonly ILogger<ProfileGrpcService> _logger;

        public ProfileGrpcService(ILogger<ProfileGrpcService> logger)
        {
            _logger = logger;
        }

    }


}