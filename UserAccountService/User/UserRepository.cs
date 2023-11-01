using Confluent.Kafka;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcSqlUserService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UserAccountService.User
{
    public class UserRepository : IUserRepository
    {
        private readonly SqlUserService.SqlUserServiceClient _grpcClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            var host = _configuration["GrpcService:Host"];
            var port = _configuration["GrpcService:Port"];
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _grpcClient = new SqlUserService.SqlUserServiceClient(channel);
            _logger = logger;
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(string email, string password, string firstName, string lastName, string dateOfBirth, string gender)
        {
            return await _grpcClient.RegisterUserAsync(new RegisterUserRequest { Email = email, Password = password, FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = gender });
        }

        public async Task<LoginResponse> LoginUserAsync(string email, string password)
        {
            return await _grpcClient.LoginAsync(new LoginRequest { Email = email, Password = password });
        }

        public async Task<ValidateTokenResponse> ValidateTokenAsync()
        {
            return await _grpcClient.ValidateTokenAsync(new ValidateTokenRequest());
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(string email, string password, string newpassword)
        {
            return await _grpcClient.ResetPasswordAsync(new ResetPasswordRequest { Email = email, CurrentPassword = password, NewPassword = newpassword });
        }

        public async Task<UserExistsResponse> UserExistsByEmailAsync(string email)
        {
            return await _grpcClient.UserExistsByEmailAsync(new UserExistsRequest { Email = email });
        }

        public async Task<DeactivateUserResponse> DeactivateUserAsync(string email, string password)
        {
            return await _grpcClient.DeactivateUserAsync(new DeactivateUserRequest { Email = email, Password = password });
        }
    }
}
