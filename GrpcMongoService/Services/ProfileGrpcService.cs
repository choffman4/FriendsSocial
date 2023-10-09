using Confluent.Kafka;
using Grpc.Core;
using GrpcMongoService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace GrpcMongoService.Services
{
    public class ProfileGrpcService : ProfileService.ProfileServiceBase
    {
        private readonly ILogger<ProfileGrpcService> _logger;
        private readonly IConfiguration _configuration;

        public ProfileGrpcService(ILogger<ProfileGrpcService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }



    }


}