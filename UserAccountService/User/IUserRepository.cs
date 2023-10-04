using GrpcUserService;
using UserAccountService.User;

public interface IUserRepository
{
    // Method for user registration
    Task<RegisterUserResponse> RegisterUserAsync(string email, string password);

    // Method for user login
    Task<LoginResponse> LoginUserAsync(string email, string password);

    // Method for token validation
    Task<ValidateTokenResponse> ValidateTokenAsync();

    // Method to request a password reset
    Task<ResetPasswordResponse> RequestPasswordResetAsync(string email);

    // Method to perform the actual password reset
    Task<PerformResetPasswordResponse> ResetPasswordAsync(string email, string token, string newPassword);
}

