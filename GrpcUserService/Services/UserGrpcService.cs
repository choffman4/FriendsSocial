using Grpc.Core;
using GrpcUserService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace GrpcUserService.Services
{
    public class UserGrpcService : UserService.UserServiceBase
    {
        private readonly ILogger<UserGrpcService> _logger;

        public UserGrpcService(ILogger<UserGrpcService> logger)
        {
            _logger = logger;
        }

        public override Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
        {
            // Your logic to register a user goes here.
            // For now, let's return a dummy token.

            var response = new RegisterUserResponse { Token = "dummy-token" };
            return Task.FromResult(response);
        }

        public override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            // Your logic to login a user goes here.
            // For now, let's return a dummy token.

            var response = new LoginResponse { Token = "dummy-token" };
            return Task.FromResult(response);
        }

        public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            // Your logic to validate a user's token goes here.
            // For now, let's return an empty response.

            var response = new ValidateTokenResponse();
            return Task.FromResult(response);
        }

        public override Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
        {
            // Your logic to request password reset goes here.
            // For now, let's return an empty response.

            var response = new ResetPasswordResponse();
            return Task.FromResult(response);
        }

        public override Task<PerformResetPasswordResponse> PerformResetPassword(PerformResetPasswordRequest request, ServerCallContext context)
        {
            // Your logic to actually reset the password after the user has gotten a reset token goes here.
            // For now, let's return an empty response.

            var response = new PerformResetPasswordResponse();
            return Task.FromResult(response);
        }
    }
}