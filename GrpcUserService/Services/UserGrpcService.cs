using Grpc.Core;
using GrpcUserService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Org.BouncyCastle.Crypto.Generators;
using BCrypt.Net;

namespace GrpcUserService.Services
{
    public class UserGrpcService : UserService.UserServiceBase
    {
        private readonly ILogger<UserGrpcService> _logger;
        private readonly IConfiguration _configuration;

        public UserGrpcService(ILogger<UserGrpcService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async override Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);

            //// Check if email exists
            string checkEmailQuery = "SELECT COUNT(1) FROM UserAccounts WHERE Email = @Email";
            MySqlCommand cmd = new MySqlCommand(checkEmailQuery, connection);
            cmd.Parameters.AddWithValue("@Email", request.Email);

            await connection.OpenAsync();

            var count = (long)await cmd.ExecuteScalarAsync();
            if (count > 0)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email already exists."));
            }

            // Generate a new GUID for the user
            string userId = Guid.NewGuid().ToString();

            // Hash the password using BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            string insertQuery = "INSERT INTO UserAccounts (UserId, Email, PasswordHash) VALUES (@UserId, @Email, @PasswordHash);";
            cmd.CommandText = insertQuery;
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@Email", request.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword); // Store the hashed password

            await cmd.ExecuteNonQueryAsync();

            var jwtToken = GenerateJwtToken(userId);
            var response = new RegisterUserResponse { Token = jwtToken };
            return response;
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

        private string GenerateJwtToken(string guid)
        {
            Console.WriteLine($"_configuration is null: {_configuration == null}"); // replace with your logging mechanism
            Console.WriteLine($"guid: {guid}");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, guid), // Replace 'user_email_or_id' with your user's email or ID
                // Add other claims as needed
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddHours(1), // Token expiry time
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}