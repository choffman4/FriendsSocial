﻿using System.Threading.Tasks;
using Grpc.Core;
using GrpcSqlUserService;
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

            var response = await _userRepository.RegisterUserAsync(request.Email, request.Password, request.FirstName, request.LastName, request.DateOfBirth, request.Gender);

            if (!string.IsNullOrEmpty(response?.Token))
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

        //[HttpPost("login")]
        //public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequest request)
        //{
        //    if (request == null)
        //    {
        //        return BadRequest("Invalid request format");
        //    }

        //    var response = await _userRepository.LoginUserAsync(request.Email, request.Password);

        //    if (response != null)
        //    {
        //        // Instead of using the JWT token, we're generating a simple session ID or using some other unique value
        //        var sessionId = Guid.NewGuid().ToString();

        //        var cookieOptions = new CookieOptions
        //        {
        //            HttpOnly = true,
        //            Secure = true,  // Set this to true if you're using HTTPS
        //            Expires = DateTimeOffset.UtcNow.AddHours(1)  // Set the expiration as per your requirement
        //        };

        //        // Store the session ID in the cookie
        //        Response.Cookies.Append("UserSessionId", sessionId, cookieOptions);

        //        // Optionally, store the session ID in some backend store (like a database or cache) 
        //        // along with the user's details if needed for further checks

        //        return Ok(new { Message = "Login successful" });
        //    } else
        //    {
        //        return BadRequest("User login failed");
        //    }
        //}

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

        [HttpPost("reset-password")]
        public async Task<IActionResult> RequestPasswordResetAsync([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            try
            {
                var response = await _userRepository.ResetPasswordAsync(request.Email, request.CurrentPassword, request.NewPassword);

                return Ok(response);
            } catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(ex.Status.Detail);
            } catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized(ex.Status.Detail);
            } catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred during password change");
            }
        }

        [HttpGet("exists/{email}")]
        public async Task<IActionResult> UserExistsByEmailAsync([FromRoute] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid email");
            }

            var response = await _userRepository.UserExistsByEmailAsync(email);

            if (response != null && response.Exists)
            {
                return Ok(new { Exists = true });
            } else
            {
                return Ok(new { Exists = false });
            }
        }

        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateUserAsync([FromBody] DeactivateUserRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid request format");
            }

            try
            {
                var response = await _userRepository.DeactivateUserAsync(request.Email, request.Password);
                return Ok(response);
            } catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
            {
                return NotFound(ex.Status.Detail);
            } catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Unauthenticated)
            {
                return Unauthorized(ex.Status.Detail);
            } catch (Exception ex)
            {
                return StatusCode(500, "Internal server error occurred during user deactivation");
            }
        }
    }
}