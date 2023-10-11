using GrpcMongoProfileService;

namespace ProfileService.Profile
{
    public interface IProfileRepository
    {
        Task<UpdateProfileResponse> UpdateProfileAsync(UpdateProfileRequest request);
        Task<GetProfileByGuidResponse> GetProfileByGuidAsync(GetProfileByGuidRequest request);
        Task<GetProfileByUsernameResponse> GetProfileByUsernameAsync(GetProfileByUsernameRequest request);
        Task<UsernameAvailabilityResponse> CheckUsernameAvailabilityAsync(UsernameAvailabilityRequest request);
    }
}
