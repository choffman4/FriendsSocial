using Grpc.Core;
using GrpcServiceTemp.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace GrpcServiceTemp.Services
{
    public class ProfileGrpcService : MongoProfileService.MongoProfileServiceBase
    {
        private readonly ILogger<ProfileGrpcService> _logger;
        private readonly IConfiguration _configuration;

        public ProfileGrpcService(ILogger<ProfileGrpcService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async override Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
        {
            // Connect to your MongoDB database and collection
            var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
            var database = mongoClient.GetDatabase("profileDatabase");
            var collection = database.GetCollection<UserProfile>("profiles");

            try
            {
                var userId = request.Guid; // Assuming the request contains the user ID



                // Find the profile by user ID
                var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
                var existingProfile = await collection.Find(filter).FirstOrDefaultAsync();

                if (existingProfile == null)
                {
                    // Handle the case where the profile doesn't exist
                    return new UpdateProfileResponse { Message = "Profile not found." };
                }

                // Update the profile fields based on the request
                existingProfile.Username = request.Username;
                existingProfile.FirstName = request.FirstName;
                existingProfile.LastName = request.LastName;
                existingProfile.Bio = request.Bio;
                existingProfile.HomeTown = request.Hometown;
                existingProfile.Occupation = request.Occupation;
                existingProfile.ExternalLink = request.ExternalLink;
                existingProfile.DateOfBirth = request.DateOfBirth;
                existingProfile.ProfilePictureUrl = request.ProfilePictureUrl;
                existingProfile.CoverPictureUrl = request.CoverPictureUrl;

                // Save the updated profile to the database
                await collection.ReplaceOneAsync(filter, existingProfile);

                return new UpdateProfileResponse { Message = "Profile updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during profile update");
                return new UpdateProfileResponse { Message = "An error occurred while updating the profile." };
            }
        }


        public async override Task<GetProfileByGuidResponse> GetProfileByGuid(GetProfileByGuidRequest request, ServerCallContext context)
        {
            _logger.LogInformation("GetProfileByGuid method called with Guid: {Guid}", request.Guid);

            // Connect to your MongoDB database and collection
            _logger.LogInformation("Connecting to MongoDB database...");
            var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
            var database = mongoClient.GetDatabase("profileDatabase");
            var collection = database.GetCollection<UserProfile>("profiles");

            try
            {


                var userId = request.Guid; // Assuming the request contains the user ID

                // Find the profile by user ID
                _logger.LogInformation("Querying MongoDB for profile...");
                var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
                var profile = await collection.Find(filter).FirstOrDefaultAsync();

                if (profile == null)
                {
                    // Handle the case where the profile doesn't exist
                    _logger.LogInformation("Profile not found.");
                    return new GetProfileByGuidResponse
                    {
                        Username = "",
                        Userid = "",
                        FirstName = "",
                        LastName = "",
                        Bio = "",
                        Hometown = "",
                        Occupation = "",
                        ExternalLink = "",
                        DateOfBirth = "",
                        ProfilePictureUrl = "",
                        CoverPictureUrl = ""
                    };
                }

                // Convert the retrieved profile to the gRPC response message
                var response = new GetProfileByGuidResponse
                {
                    Userid = profile.UserId,
                    Username = profile.Username,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Bio = profile.Bio,
                    Hometown = profile.HomeTown,
                    Occupation = profile.Occupation,
                    ExternalLink = profile.ExternalLink,
                    DateOfBirth = profile.DateOfBirth,
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    CoverPictureUrl = profile.CoverPictureUrl
                };

                _logger.LogInformation("Profile found and returned.");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the profile");
                return new GetProfileByGuidResponse
                {
                    Username = "",
                    Userid = "",
                    FirstName = "",
                    LastName = "",
                    Bio = "",
                    Hometown = "",
                    Occupation = "",
                    ExternalLink = "",
                    DateOfBirth = "",
                    ProfilePictureUrl = "",
                    CoverPictureUrl = ""
                };
            }
        }


        public async override Task<UsernameAvailabilityResponse> CheckUsernameAvailability(UsernameAvailabilityRequest request, ServerCallContext context)
        {
            // Connect to your MongoDB database and collection
            var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
            var database = mongoClient.GetDatabase("profileDatabase");
            var collection = database.GetCollection<UserProfile>("profiles");


            try
            {
                var username = request.Username; // Assuming the request contains the username to check


                // Check if a profile with the provided username already exists
                var filter = Builders<UserProfile>.Filter.Eq(u => u.Username, username);
                var existingProfile = await collection.Find(filter).FirstOrDefaultAsync();

                // Create a response indicating whether the username is available or not
                var response = new UsernameAvailabilityResponse { Available = existingProfile == null };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking username availability");
                return new UsernameAvailabilityResponse { Available = false }; // Return false on error
            }
        }

        public async override Task<GetProfileByUsernameResponse> GetProfileByUsername(GetProfileByUsernameRequest request, ServerCallContext context)
        {
            var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
            var database = mongoClient.GetDatabase("profileDatabase");
            var collection = database.GetCollection<UserProfile>("profiles");

            try
            {
                var username = request.Username; // Assuming the request contains the username to check



                // Find the profile by username
                var filter = Builders<UserProfile>.Filter.Eq(u => u.Username, username);
                var profile = await collection.Find(filter).FirstOrDefaultAsync();

                if (profile == null)
                {
                    // Handle the case where the profile doesn't exist
                    return new GetProfileByUsernameResponse
                    {
                        Userid = "",
                        Username = "",
                        FirstName = "",
                        LastName = "",
                        Bio = "",
                        Hometown = "",
                        Occupation = "",
                        ExternalLink = "",
                        DateOfBirth = "",
                        ProfilePictureUrl = "",
                        CoverPictureUrl = ""
                    };
                }

                // Convert the retrieved profile to the gRPC response message
                var response = new GetProfileByUsernameResponse
                {
                    Userid = profile.UserId,
                    Username = profile.Username,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Bio = profile.Bio,
                    Hometown = profile.HomeTown,
                    Occupation = profile.Occupation,
                    ExternalLink = profile.ExternalLink,
                    DateOfBirth = profile.DateOfBirth,
                    ProfilePictureUrl = profile.ProfilePictureUrl,
                    CoverPictureUrl = profile.CoverPictureUrl
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the profile by username");
                return new GetProfileByUsernameResponse
                {
                    Userid = "",
                    Username = "",
                    FirstName = "",
                    LastName = "",
                    Bio = "",
                    Hometown = "",
                    Occupation = "",
                    ExternalLink = "",
                    DateOfBirth = "",
                    ProfilePictureUrl = "",
                    CoverPictureUrl = ""
                };
            }
        }
    }
}