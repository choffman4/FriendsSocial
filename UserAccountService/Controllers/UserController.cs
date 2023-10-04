using System.Threading.Tasks;
using GrpcUserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserAccountService.User;

namespace UserAccountService.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterUserRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            var response = await _userRepository.RegisterUserAsync(request.Email, request.Password);

            if (response != null)
            {
                return Ok(response);
            } else
            {
                return BadRequest("User registration failed");
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            var response = await _userRepository.LoginUserAsync(request.Email, request.Password);

            if (response != null)
            {
                return Ok(response);
            } else
            {
                return BadRequest("User login failed");
            }
        }

        [HttpPost("logout")]
        [Authorize] // You can add authentication middleware to protect this endpoint.
        public IActionResult LogoutUser()
        {
            // Implement your logic to log the user out and invalidate the token.
            // Return an appropriate response.
            return NoContent();
        }

        [HttpGet("validate")]
        [Authorize] // You can add authentication middleware to protect this endpoint.
        public async Task<IActionResult> ValidateTokenAsync()
        {
            var response = await _userRepository.ValidateTokenAsync();

            if (response != null)
            {
                return Ok(response);
            } else
            {
                return Unauthorized(); // Return 401 if token is invalid.
            }
        }

        [HttpPost("reset-password/request")]
        public async Task<IActionResult> RequestPasswordResetAsync([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            var response = await _userRepository.RequestPasswordResetAsync(request.Email);

            if (response != null)
            {
                return Ok(response);
            } else
            {
                return BadRequest("Password reset request failed");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] PerformResetPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            var response = await _userRepository.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

            if (response != null)
            {
                return Ok(response);
            } else
            {
                return BadRequest("Password reset failed");
            }
        }
    }
}