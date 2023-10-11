using GrpcSqlUserService;
using UserAccountService.User;

public interface IUserRepository
{
    // Method for user registration
    Task<RegisterUserResponse> RegisterUserAsync(string email, string password);

    // Method for user login
    Task<LoginResponse> LoginUserAsync(string email, string password);

    // Method for token validation
    Task<ValidateTokenResponse> ValidateTokenAsync();

    // Method to password reset
    Task<ResetPasswordResponse> ResetPasswordAsync(string email, string password, string newpassword);

    // Method to check if a user exists by email
    Task<UserExistsResponse> UserExistsByEmailAsync(string email);

    // method to deactivate user account
    Task<DeactivateUserResponse> DeactivateUserAsync(string email, string password);

}

