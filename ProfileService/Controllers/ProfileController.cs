using GrpcMongoProfileService;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Profile;
using System.Threading.Tasks;



[Route("profile")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly IProfileRepository _profileRepository;

    public ProfileController(IProfileRepository profileRepository)
    {
        _profileRepository = profileRepository;
    }

    [HttpPost("update")]
    public async Task<IActionResult> UpdateProfileAsync([FromBody] UpdateProfileRequest request)
    {
        var response = await _profileRepository.UpdateProfileAsync(request);
        return Ok(response);
    }

    [HttpGet("userid/{userGuid}")]
    public async Task<IActionResult> GetProfileAsync(string userGuid)
    {
        var request = new GetProfileByGuidRequest { Guid = userGuid };
        var response = await _profileRepository.GetProfileByGuidAsync(request);
        return Ok(response);
    }

    [HttpGet("check-username")]
    public async Task<IActionResult> CheckUsernameAvailabilityAsync(string username)
    {
        var request = new UsernameAvailabilityRequest { Username = username };
        var response = await _profileRepository.CheckUsernameAvailabilityAsync(request);
        return Ok(response);
    }

    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetProfileByUsernameAsync(string username)
    {
        var request = new GetProfileByUsernameRequest { Username = username };
        var response = await _profileRepository.GetProfileByUsernameAsync(request);
        return Ok(response);
    }

    [HttpGet("search/{searchTerm}")]
    public async Task<IActionResult> SearchProfilesAsync(string searchTerm)
    {
        var request = new GetProfileSearchRequest { SearchString = searchTerm };
        var responses = new List<GetProfileSearchResponse>();

        await foreach (var response in _profileRepository.GetProfileSearchAsync(request))
        {
            responses.Add(response);
        }

        return Ok(responses);
    }



}