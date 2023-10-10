using Confluent.Kafka;
using Grpc.Core;
using GrpcMongoService;
using GrpcMongoService.User;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

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

        public async override Task<UpdateProfileResponse> UpdateProfile(UpdateProfileRequest request, ServerCallContext context)
        {
            try
            {
                var userId = request.Guid; // Assuming the request contains the user ID

                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileDatabase");
                var collection = database.GetCollection<UserProfile>("profiles");

                // Find the profile by user ID
                var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
                var existingProfile = await collection.Find(filter).FirstOrDefaultAsync();

                if (existingProfile == null)
                {
                    // Handle the case where the profile doesn't exist
                    return new UpdateProfileResponse { Message = "Profile not found." };
                }

                // Update the profile fields based on the request
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
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during profile update");
                return new UpdateProfileResponse { Message = "An error occurred while updating the profile." };
            }
        }


        public async override Task<GetProfileResponse> GetProfile(GetProfileRequest request, ServerCallContext context)
        {
            try
            {
                var userId = request.Guid; // Assuming the request contains the user ID

                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDB"));
                var database = mongoClient.GetDatabase("profileDatabase");
                var collection = database.GetCollection<UserProfile>("profiles");

                // Find the profile by user ID
                var filter = Builders<UserProfile>.Filter.Eq(u => u.UserId, userId);
                var profile = await collection.Find(filter).FirstOrDefaultAsync();

                if (profile == null)
                {
                    // Handle the case where the profile doesn't exist
                    return new GetProfileResponse { FirstName = "", LastName = "", Bio = "", Hometown = "", Occupation = "", ExternalLink = "", DateOfBirth = "", ProfilePictureUrl = "", CoverPictureUrl = "" };
                }

                // Convert the retrieved profile to the gRPC response message
                var response = new GetProfileResponse
                {
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
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching the profile");
                return new GetProfileResponse { FirstName = "", LastName = "", Bio = "", Hometown = "", Occupation = "", ExternalLink = "", DateOfBirth = "", ProfilePictureUrl = "", CoverPictureUrl = "" };
            }
        }



    }


}