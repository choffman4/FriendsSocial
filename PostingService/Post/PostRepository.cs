using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GrpcMongoPostingService;
using Confluent.Kafka;

namespace PostingService.Post
{
    public class PostRepository : IPostRepository
    {
        private readonly MongoPostingService.MongoPostingServiceClient _grpcClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PostRepository> _logger;

        public PostRepository(ILogger<PostRepository> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            var host = _configuration["GrpcService:Host"];
            var port = _configuration["GrpcService:Port"];
            var channel = new Channel($"{host}:{port}", ChannelCredentials.Insecure);
            _grpcClient = new MongoPostingService.MongoPostingServiceClient(channel);
            _logger = logger;
        }

        public async Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request)
        {
            _logger.LogInformation("CreatePostAsync called with request: {@Request}", request);

            try
            {
                _logger.LogInformation("Creating gRPC channel with host: {Host} and port: {Port}", _configuration["GrpcService:Host"], _configuration["GrpcService:Port"]);
                var response = await _grpcClient.CreatePostAsync(request);
                _logger.LogInformation("gRPC call successful, response received.");
                return response;
            } catch (RpcException ex)
            {
                _logger.LogError("gRPC error calling CreatePostAsync: {StatusCode} - {Message}", ex.StatusCode, ex.Message);
                if (ex.Status.Detail != null)
                {
                    _logger.LogError("gRPC error detail: {Detail}", ex.Status.Detail);
                }

                throw new Exception($"An error occurred while calling the gRPC service: {ex.Message}", ex);
            } catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
                throw;
            }
        }


        public async Task<GetPostByIdResponse> GetPostByIdAsync(GetPostByIdRequest request)
        {
            try
            {
                var response = await _grpcClient.GetPostByIdAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async IAsyncEnumerable<GetAllPostsByUserIdResponse> GetAllPostsByUserIdAsync(GetAllPostsByUserIdRequest request)
        {
            List<GetAllPostsByUserIdResponse> responses = new List<GetAllPostsByUserIdResponse>();

            try
            {
                using (var call = _grpcClient.GetAllPostsByUserId(request))
                {
                    await foreach (var response in call.ResponseStream.ReadAllAsync())
                    {
                        responses.Add(response);
                    }
                }
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }

            foreach (var response in responses)
            {
                yield return response;
            }
        }

        public async Task<EditPostResponse> EditPostAsync(EditPostRequest request)
        {
            try
            {
                var response = await _grpcClient.EditPostAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<DeletePostResponse> DeletePostAsync(DeletePostRequest request)
        {
            try
            {
                var response = await _grpcClient.DeletePostAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<LikePostResponse> LikePostAsync(LikePostRequest request)
        {
            try
            {
                var response = await _grpcClient.LikePostAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<AddCommentResponse> AddCommentAsync(AddCommentRequest request)
        {
            try
            {
                var response = await _grpcClient.AddCommentAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<DeleteCommentResponse> DeleteCommentAsync(DeleteCommentRequest request)
        {
            try
            {
                var response = await _grpcClient.DeleteCommentAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<LikeCommentResponse> LikeCommentAsync(LikeCommentRequest request)
        {
            try
            {
                var response = await _grpcClient.LikeCommentAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<AddCommentToCommentResponse> AddCommentToCommentAsync(AddCommentToCommentRequest request)
        {
            try
            {
                var response = await _grpcClient.AddCommentToCommentAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<EditCommentResponse> EditCommentAsync(EditCommentRequest request)
        {
            try
            {
                var response = await _grpcClient.EditCommentAsync(request);
                return response;
            } catch (RpcException ex)
            {
                // Handle exceptions (e.g., gRPC communication errors) here
                throw ex;
            }
        }

        public async Task<GetCommentResponse> GetCommentAsync(GetCommentRequest request)
        {
            try
            {
                // Make a gRPC call to the server to retrieve the comment by its commentId
                var response = await _grpcClient.GetCommentAsync(request);

                // Convert the gRPC response to your local response type
                var commentResponse = new GetCommentResponse
                {
                    Userid = response.Userid,
                    Content = response.Content,
                    Date = response.Date,
                    ChildCommentIds = { response.ChildCommentIds },
                    Likes = { response.Likes }
                };

                return commentResponse;
            } catch (RpcException ex)
            {
                // Handle gRPC exceptions here
                Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
                throw;
            } catch (Exception ex)
            {
                // Handle other exceptions here
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }

        public async Task<GetPostsResponse> GetPostsAsync(GetPostsRequest request)
        {
            try
            {
                var response = await _grpcClient.GetPostsAsync(request); // Ensure this method is asynchronous
                return response;
            } catch (RpcException ex)
            {
                throw ex;
            }
        }

    }
}
