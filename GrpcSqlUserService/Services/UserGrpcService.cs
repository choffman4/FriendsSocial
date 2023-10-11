using Grpc.Core;
using GrpcSqlUserService;
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
using System.Data;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using GrpcSqlUserService.Kafka;

namespace GrpcSqlUserService.Services
{
    public class UserGrpcService : SqlUserService.SqlUserServiceBase
    {
        private readonly ILogger<UserGrpcService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProducer<string, string> _kafkaProducer;

        public UserGrpcService(ILogger<UserGrpcService> logger, IConfiguration configuration, IOptions<KafkaSettings> kafkaSettings)
        {
            _logger = logger;
            _configuration = configuration;
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = kafkaSettings.Value.BootstrapServers
            };
            _kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
        }

        public async override Task<RegisterUserResponse> RegisterUser(RegisterUserRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Check if email exists
                string checkEmailQuery = "SELECT COUNT(1) FROM UserAccounts WHERE Email = @Email";
                MySqlCommand cmd = new MySqlCommand(checkEmailQuery, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

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


                await connection.CloseAsync();

                await SendToKafka(userId);

                var jwtToken = GenerateJwtToken(userId);
                return new RegisterUserResponse { Token = jwtToken };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user registration");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during registration"));
            }
        }

        public async override Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Retrieve user by email
                string getUserQuery = "SELECT UserId, PasswordHash, IsActive FROM UserAccounts WHERE Email = @Email";
                MySqlCommand cmd = new MySqlCommand(getUserQuery, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
                }

                await reader.ReadAsync();
                string userId = reader.GetString("UserId");
                string storedHash = reader.GetString("PasswordHash");
                bool isActive = reader.GetBoolean("IsActive");

                // Close the DataReader before executing another query
                reader.Close();

                if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password."));
                }

                // If the account is not active, activate it and send a Kafka message
                if (!isActive)
                {
                    string activateUserQuery = "UPDATE UserAccounts SET IsActive = 1 WHERE UserId = @UserId";
                    cmd = new MySqlCommand(activateUserQuery, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    await cmd.ExecuteNonQueryAsync();

                    // Send a Kafka message indicating user activation
                    await SendUserActivationMessage(userId, isDeactivation: false);
                }

                var jwtToken = GenerateJwtToken(userId);
                return new LoginResponse { Token = jwtToken };
            } catch (RpcException)
            {
                // Re-throw any RPC exceptions that we explicitly created
                throw;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user login");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during login"));
            } finally
            {
                await connection.CloseAsync();
            }
        }

        public override Task<ValidateTokenResponse> ValidateToken(ValidateTokenRequest request, ServerCallContext context)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            // Retrieve the token from the gRPC metadata
            var metadata = context.RequestHeaders;
            var tokenEntry = metadata.FirstOrDefault(m => m.Key == "authorization");
            if (tokenEntry == null)
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token is missing from headers."));
            }

            var token = tokenEntry.Value;
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Token is missing or empty."));
            }

            try
            {
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // This will throw if the token is invalid
                jwtTokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                return Task.FromResult(new ValidateTokenResponse()); // Return an empty response for a valid token
            } catch (SecurityTokenException)
            {
                // Token is invalid
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid or expired token."));
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during token validation");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during token validation"));
            }
        }

        public async override Task<ResetPasswordResponse> ResetPassword(ResetPasswordRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Fetch the hashed password from the database using the Email
                string fetchPasswordQuery = "SELECT PasswordHash FROM UserAccounts WHERE Email = @Email";
                MySqlCommand cmd = new MySqlCommand(fetchPasswordQuery, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

                var storedHashedPassword = (string?)await cmd.ExecuteScalarAsync();

                if (storedHashedPassword == null)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
                }

                // Check if the current password provided by the user matches the stored hashed password
                bool isValidPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, storedHashedPassword);

                if (!isValidPassword)
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Current password is incorrect."));
                }

                // Hash the new password and update the record in the database
                string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

                string updatePasswordQuery = "UPDATE UserAccounts SET PasswordHash = @NewPasswordHash WHERE Email = @Email;";
                cmd.CommandText = updatePasswordQuery;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Email", request.Email);
                cmd.Parameters.AddWithValue("@NewPasswordHash", newHashedPassword);

                await cmd.ExecuteNonQueryAsync();

                var response = new ResetPasswordResponse { Message = "Password has been successfully updated." };
                return response;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during password reset");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during password reset"));
            }
        }

        public async override Task<UserExistsResponse> UserExistsByEmail(UserExistsRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Check if email exists
                string checkEmailQuery = "SELECT COUNT(1) FROM UserAccounts WHERE Email = @Email";
                MySqlCommand cmd = new MySqlCommand(checkEmailQuery, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

                var count = (long)await cmd.ExecuteScalarAsync();
                await connection.CloseAsync();

                var exists = count > 0;  // Convert count to a boolean value

                return new UserExistsResponse { Exists = exists };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during email existence check");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during email check"));
            }
        }

        private string GenerateJwtToken(string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, userId)
            // Add other claims as needed
        }),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration time (you can adjust this as needed)
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task SendToKafka(string userId)
        {
            try
            {
                await _kafkaProducer.ProduceAsync("RegisterUser", new Message<string, string> { Key = null, Value = userId });
                _logger.LogInformation("Message sent to Kafka.");
                //_kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            } catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(ex, "Error occurred while sending message to Kafka.");
                // Handle the exception as needed.
            }
        }

        public async override Task<DeactivateUserResponse> DeactivateUser(DeactivateUserRequest request, ServerCallContext context)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using MySqlConnection connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            try
            {
                // Fetch the hashed password from the database using the Email
                string fetchPasswordQuery = "SELECT UserId, PasswordHash FROM UserAccounts WHERE Email = @Email AND IsActive = 1";
                MySqlCommand cmd = new MySqlCommand(fetchPasswordQuery, connection);
                cmd.Parameters.AddWithValue("@Email", request.Email);

                using var reader = await cmd.ExecuteReaderAsync();

                if (!reader.HasRows)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found or already deactivated."));
                }

                await reader.ReadAsync();
                string userId = reader.GetString("UserId");
                string storedHash = reader.GetString("PasswordHash");

                if (!BCrypt.Net.BCrypt.Verify(request.Password, storedHash))
                {
                    throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password."));
                }

                // If password verification is successful, deactivate the user
                reader.Close();

                string deactivateQuery = "UPDATE UserAccounts SET IsActive = 0 WHERE UserId = @UserId";
                cmd = new MySqlCommand(deactivateQuery, connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                await cmd.ExecuteNonQueryAsync();

                await connection.CloseAsync();

                // Send a Kafka message for user deactivation
                await SendUserActivationMessage(userId, isDeactivation: true);

                return new DeactivateUserResponse { Message = "User has been successfully deactivated." };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during user deactivation");
                throw new RpcException(new Status(StatusCode.Internal, "Internal server error occurred during user deactivation"));
            }
        }

        private async Task SendUserActivationMessage(string userId, bool isDeactivation)
        {
            try
            {
                // Send a Kafka message with user activation information
                string topicName = isDeactivation ? "UserDeactivation" : "UserActivation";
                await _kafkaProducer.ProduceAsync(topicName, new Message<string, string> { Key = null, Value = userId });
                _kafkaProducer.Flush(TimeSpan.FromSeconds(10));
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending user activation/deactivation message to Kafka.");
                // Handle any Kafka message sending errors as needed
            }
        }
    }
}