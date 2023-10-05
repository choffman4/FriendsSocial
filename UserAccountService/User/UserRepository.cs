using Confluent.Kafka;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcUserService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UserAccountService.User
{
    public class UserRepository : IUserRepository
    {
        private readonly UserService.UserServiceClient _grpcClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(ILogger<UserRepository> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            var host = _configuration["GrpcService:Host"];
            var port = _configuration["GrpcService:Port"];
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _grpcClient = new UserService.UserServiceClient(channel);
            _logger = logger;
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(string email, string password)
        {
            return await _grpcClient.RegisterUserAsync(new RegisterUserRequest { Email = email, Password = password });
        }

        public async Task<LoginResponse> LoginUserAsync(string email, string password)
        {
            return await _grpcClient.LoginAsync(new LoginRequest { Email = email, Password = password });
        }

        public async Task<ValidateTokenResponse> ValidateTokenAsync()
        {
            return await _grpcClient.ValidateTokenAsync(new ValidateTokenRequest());
        }

        public async Task<ResetPasswordResponse> RequestPasswordResetAsync(string email)
        {
            return await _grpcClient.ResetPasswordAsync(new ResetPasswordRequest { Email = email });
        }

        public async Task<PerformResetPasswordResponse> ResetPasswordAsync(string email, string token, string newPassword)
        {
            return await _grpcClient.PerformResetPasswordAsync(new PerformResetPasswordRequest
            {
                Email = email,
                Token = token,
                NewPassword = newPassword
            });
        }

        public class UserAlreadyExistsException : Exception
        {
            public UserAlreadyExistsException() : base() { }

            public UserAlreadyExistsException(string message) : base(message) { }

            // You can also add more constructors based on your needs
        }
    }
}
