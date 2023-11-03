using Grpc.Core;
using GrpcMongoPostingService;
using Microsoft.AspNetCore.Mvc;
using PostingService.Post;

namespace PostingService.Controllers
{
    [Route("post")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        [HttpGet("GetPosts")]
        public async Task<ActionResult<GetPostsResponse>> GetPosts([FromQuery] GetPostsRequest request)
        {
            try
            {
                var response = await _postRepository.GetPostsAsync(request);
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            } catch (RpcException ex)
            {
                // Log the exception details, etc.
                return StatusCode((int)ex.StatusCode, ex.Status.Detail);
            } catch (Exception ex)
            {
                // Log the exception details, handle the error appropriately
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost("CreatePost")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
        {
            var response = await _postRepository.CreatePostAsync(request);
            return Ok(response);
        }

        [HttpGet("GetPostById")]
        public async Task<IActionResult> GetPostById([FromBody] GetPostByIdRequest request)
        {
            var response = await _postRepository.GetPostByIdAsync(request);
            return Ok(response);
        }

        [HttpGet("GetAllPostsByUserId")]
        public async Task<IActionResult> GetAllPostsByUserId([FromBody] GetAllPostsByUserIdRequest request)
        {
            var responses = await _postRepository.GetAllPostsByUserIdAsync(request).ToListAsync();
            return Ok(responses);
        }

        [HttpPost("EditPost")]
        public async Task<IActionResult> EditPost([FromBody] EditPostRequest request)
        {
            var response = await _postRepository.EditPostAsync(request);
            return Ok(response);
        }

        [HttpPost("DeletePost")]
        public async Task<IActionResult> DeletePost([FromBody] DeletePostRequest request)
        {
            var response = await _postRepository.DeletePostAsync(request);
            return Ok(response);
        }

        [HttpPost("LikePost")]
        public async Task<IActionResult> LikePost([FromBody] LikePostRequest request)
        {
            var response = await _postRepository.LikePostAsync(request);
            return Ok(response);
        }

        [HttpPost("AddComment")]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            var response = await _postRepository.AddCommentAsync(request);
            return Ok(response);
        }

        [HttpPost("DeleteComment")]
        public async Task<IActionResult> DeleteComment([FromBody] DeleteCommentRequest request)
        {
            var response = await _postRepository.DeleteCommentAsync(request);
            return Ok(response);
        }

        [HttpPost("LikeComment")]
        public async Task<IActionResult> LikeComment([FromBody] LikeCommentRequest request)
        {
            var response = await _postRepository.LikeCommentAsync(request);
            return Ok(response);
        }

        [HttpPost("AddCommentToComment")]
        public async Task<IActionResult> AddCommentToComment([FromBody] AddCommentToCommentRequest request)
        {
            var response = await _postRepository.AddCommentToCommentAsync(request);
            return Ok(response);
        }

        [HttpPost("EditComment")]
        public async Task<IActionResult> EditComment([FromBody] EditCommentRequest request)
        {
            var response = await _postRepository.EditCommentAsync(request);
            return Ok(response);
        }

        [HttpGet("GetComment")]
        public async Task<IActionResult> GetComment([FromBody] GetCommentRequest request)
        {
            try
            {

                // Call the GetCommentAsync method from your repository
                var response = await _postRepository.GetCommentAsync(request);

                // Check if the comment was found
                if (!string.IsNullOrEmpty(response.Userid))
                {
                    // Return the comment as an HTTP response
                    return Ok(response);
                } else
                {
                    // Comment not found, return an appropriate HTTP response
                    return NotFound("Comment not found.");
                }
            } catch (Exception ex)
            {
                // Handle exceptions and log errors
                return StatusCode(500, "An error occurred while fetching the comment.");
            }
        }
    }
}
