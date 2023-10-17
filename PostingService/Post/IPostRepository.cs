using GrpcMongoPostingService;

namespace PostingService.Post
{
    public interface IPostRepository
    {
        Task<CreatePostResponse> CreatePostAsync(CreatePostRequest request);
        Task<GetPostByIdResponse> GetPostByIdAsync(GetPostByIdRequest request);
        IAsyncEnumerable<GetAllPostsByUserIdResponse> GetAllPostsByUserIdAsync(GetAllPostsByUserIdRequest request);
        Task<EditPostResponse> EditPostAsync(EditPostRequest request);
        Task<DeletePostResponse> DeletePostAsync(DeletePostRequest request);
        Task<LikePostResponse> LikePostAsync(LikePostRequest request);
        Task<AddCommentResponse> AddCommentAsync(AddCommentRequest request);
        Task<DeleteCommentResponse> DeleteCommentAsync(DeleteCommentRequest request);
        Task<LikeCommentResponse> LikeCommentAsync(LikeCommentRequest request);
        Task<AddCommentToCommentResponse> AddCommentToCommentAsync(AddCommentToCommentRequest request);
        Task<EditCommentResponse> EditCommentAsync(EditCommentRequest request);
        Task<GetCommentResponse> GetCommentAsync(GetCommentRequest request);
    }
}
