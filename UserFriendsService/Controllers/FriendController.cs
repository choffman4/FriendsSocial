using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UserFriendsService.Friend;
using GrpcMongoFriendService;

namespace UserFriendsService.Controllers
{
    [Route("friends")]
    [ApiController]
    public class FriendController : Controller
    {
        private readonly IFriendRepository _friendRepository;

        public FriendController(IFriendRepository friendRepository)
        {
            _friendRepository = friendRepository;
        }

        [HttpPost("AcceptFriendRequest")]
        public async Task<IActionResult> AcceptFriendRequest([FromBody] AcceptFriendRequestRequest request)
        {
            var response = await _friendRepository.AcceptFriendRequestAsync(request);
            return Ok(response);
        }

        [HttpGet("GetFriendRequestsReceived")]
        public async Task<IActionResult> GetFriendRequestsReceived([FromBody] GetFriendRequestsReceivedRequest request)
        {
            var responses = await _friendRepository.GetFriendRequestsReceivedAsync(request).ToListAsync();
            return Ok(responses);
        }

        [HttpGet("GetFriendRequestsSent")]
        public async Task<IActionResult> GetFriendRequestsSent([FromBody] GetFriendRequestsSentRequest request)
        {
            var responses = await _friendRepository.GetFriendRequestsSentAsync(request).ToListAsync();
            return Ok(responses);
        }

        [HttpGet("GetFriends")]
        public async Task<IActionResult> GetFriends([FromBody] GetFriendsRequest request)
        {
            var responses = await _friendRepository.GetFriendsAsync(request).ToListAsync();
            return Ok(responses);
        }

        [HttpPost("RemoveFriend")]
        public async Task<IActionResult> RemoveFriend([FromBody] RemoveFriendRequest request)
        {
            var response = await _friendRepository.RemoveFriendAsync(request);
            return Ok(response);
        }

        [HttpPost("SendFriendRequest")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestRequest request)
        {
            var response = await _friendRepository.SendFriendRequestAsync(request);
            return Ok(response);
        }

        [HttpGet("GetFriendship")]
        public async Task<IActionResult> GetFriendship([FromBody] GetFriendshipRequest request)
        {
            try
            {
                // Call the GetFriendshipAsync method from your repository
                var response = await _friendRepository.GetFriendshipAsync(request);

                // Check the response for errors or success and return an appropriate result
                if (!string.IsNullOrEmpty(response.Message))
                {
                    return Ok(response);
                } else
                {
                    // Handle the case where the response is empty or contains an error message
                    return BadRequest("Failed to get friendship information.");
                }
            } catch (Exception ex)
            {
                // Handle any exceptions that occur during the operation
                // You can log the exception or return an appropriate error response
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
    }
}
