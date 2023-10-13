using Grpc.Core;
using MongoDB.Driver;
using GrpcMongoPostingService.PostProperties;

namespace GrpcMongoPostingService.Services
{
    public class PostGrpcService : MongoPostingService.MongoPostingServiceBase
    {
        private readonly ILogger<PostGrpcService> _logger;
        private readonly IConfiguration _configuration;

        public PostGrpcService(ILogger<PostGrpcService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var collection = database.GetCollection<Post>("posts");

                // Create a new Post object based on the request
                var newPost = new Post(
                    userid: request.Userid,
                    title: request.Title,
                    content: request.Content,
                    privacyType: request.PrivacyType
                );

                // Insert the new post into the MongoDB collection
                await collection.InsertOneAsync(newPost);

                return new CreatePostResponse { Message = "Post created successfully." };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new post.");
                return new CreatePostResponse { Message = "An error occurred while creating the post." };
            }
        }

        public override async Task<GetPostByIdResponse> GetPostById(GetPostByIdRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts"); // Use the appropriate database name
                var postCollection = database.GetCollection<Post>("posts");
                var commentCollection = database.GetCollection<Comment>("comments");

                // Find the post by its PostId
                var postFilter = Builders<Post>.Filter.Eq(p => p.PostId, request.Postid);
                var post = await postCollection.Find(postFilter).FirstOrDefaultAsync();

                if (post != null)
                {
                    // Convert the post to the response message
                    var response = new GetPostByIdResponse
                    {
                        Userid = post.UserId,
                        Title = post.Title,
                        Content = post.Content,
                        Date = post.PostedDate.ToString(), // Convert the date to string
                        ChildCommentIds = { } // Initialize the repeated field
                    };

                    // Loop through post.ChildCommentIds and add each comment ID to the list of child comment IDs in the response
                    foreach (var childCommentId in post.ChildCommentIds)
                    {
                        // Find the associated comment by its CommentId
                        var commentFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, childCommentId);
                        var comment = await commentCollection.Find(commentFilter).FirstOrDefaultAsync();

                        if (comment != null)
                        {
                            // Add the child comment ID to the response
                            response.ChildCommentIds.Add(childCommentId);
                        }
                    }

                    return response;
                } else
                {
                    return new GetPostByIdResponse { Message = "Post not found." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting a post by ID.");
                return new GetPostByIdResponse { Message = "An error occurred while getting the post." };
            }
        }

        public override async Task GetAllPostsByUserId(GetAllPostsByUserIdRequest request, IServerStreamWriter<GetAllPostsByUserIdResponse> responseStream, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profileDatabase"); // Use the appropriate database name
                var databasePosts = mongoClient.GetDatabase("profilePosts");
                var postCollection = databasePosts.GetCollection<Post>("posts");
                var commentCollection = databasePosts.GetCollection<Comment>("comments");

                // Find all posts by the user ID
                var postFilter = Builders<Post>.Filter.Eq(p => p.UserId, request.Userid);
                var postCursor = await postCollection.Find(postFilter).ToCursorAsync();

                // Iterate through the posts and stream each one
                foreach (var post in postCursor.ToEnumerable())
                {
                    // Convert the post to the response message
                    var response = new GetAllPostsByUserIdResponse
                    {
                        Userid = post.UserId,
                        Title = post.Title,
                        Content = post.Content,
                        Date = post.PostedDate.ToString(), // Convert the date to string
                        ChildCommentIds = { } // Initialize the repeated field
                    };

                    // Loop through post.ChildCommentIds and add each comment ID to the list of child comment IDs in the response
                    foreach (var childCommentId in post.ChildCommentIds)
                    {
                        // Find the associated comment by its CommentId
                        var commentFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, childCommentId);
                        var comment = await commentCollection.Find(commentFilter).FirstOrDefaultAsync();

                        if (comment != null)
                        {
                            // Add the child comment ID to the response
                            response.ChildCommentIds.Add(childCommentId);
                        }
                    }

                    // Write the response to the stream
                    await responseStream.WriteAsync(response);
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while streaming posts by user ID.");
            }
        }


        public override async Task<EditPostResponse> EditPost(EditPostRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var collection = database.GetCollection<Post>("posts");

                // Find the post by post ID
                var filter = Builders<Post>.Filter.Eq(p => p.PostId, request.Postid);
                var existingPost = await collection.Find(filter).FirstOrDefaultAsync();

                if (existingPost == null)
                {
                    return new EditPostResponse { Message = "Post not found." };
                }

                // Update the post fields based on the request
                existingPost.Title = request.Title;
                existingPost.Content = request.Content;
                existingPost.PrivacyType = request.PrivacyType;
                existingPost.LastEditedDate = DateTime.Now.Date;

                // Update the post in the database
                var updateResult = await collection.ReplaceOneAsync(filter, existingPost);

                if (updateResult.ModifiedCount == 1)
                {
                    return new EditPostResponse { Message = "Post updated successfully." };
                } else
                {
                    return new EditPostResponse { Message = "Failed to update the post." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing a post.");
                return new EditPostResponse { Message = "An error occurred while editing the post." };
            }
        }


        public override async Task<DeletePostResponse> DeletePost(DeletePostRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var postCollection = database.GetCollection<Post>("posts");
                var commentCollection = database.GetCollection<Comment>("comments");

                // Find the post by post ID
                var postFilter = Builders<Post>.Filter.Eq(p => p.PostId, request.Postid);
                var deleteResult = await postCollection.DeleteOneAsync(postFilter);

                // Delete comments with the matching ParentPostId
                var commentFilter = Builders<Comment>.Filter.Eq(c => c.ParentPostId, request.Postid);
                await commentCollection.DeleteManyAsync(commentFilter);

                if (deleteResult.DeletedCount == 1)
                {
                    return new DeletePostResponse { Message = "Post deleted successfully." };
                } else
                {
                    return new DeletePostResponse { Message = "Post not found or failed to delete." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a post.");
                return new DeletePostResponse { Message = "An error occurred while deleting the post." };
            }
        }


        public override async Task<LikePostResponse> LikePost(LikePostRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var collection = database.GetCollection<Post>("posts");

                // Find the post by post ID
                var filter = Builders<Post>.Filter.Eq(p => p.PostId, request.Postid);
                var post = await collection.Find(filter).FirstOrDefaultAsync();

                if (post == null)
                {
                    return new LikePostResponse { Message = "Post not found." };
                }

                // Check if the UserLikesIds array is null and initialize it with an empty list if it is
                if (post.UserIdLikes == null)
                {
                    post.UserIdLikes = new List<string>();
                }

                // Check if the user has already liked the post
                if (post.UserIdLikes.Contains(request.Userid))
                {
                    // User has already liked the post, remove their like
                    post.UserIdLikes.Remove(request.Userid);

                    var update = Builders<Post>.Update.Set(p => p.UserIdLikes, post.UserIdLikes);

                    var updateResult = await collection.UpdateOneAsync(filter, update);

                    if (updateResult.ModifiedCount == 1)
                    {
                        return new LikePostResponse { Message = "Post unliked successfully." };
                    } else
                    {
                        return new LikePostResponse { Message = "Failed to unlike the post." };
                    }
                } else
                {
                    // If the user hasn't liked the post yet, add the like
                    post.UserIdLikes.Add(request.Userid);

                    var update = Builders<Post>.Update.Set(p => p.UserIdLikes, post.UserIdLikes);

                    var updateResult = await collection.UpdateOneAsync(filter, update);

                    if (updateResult.ModifiedCount == 1)
                    {
                        return new LikePostResponse { Message = "Post liked successfully." };
                    } else
                    {
                        return new LikePostResponse { Message = "Failed to like the post." };
                    }
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while liking/unliking a post.");
                return new LikePostResponse { Message = "An error occurred while liking/unliking the post." };
            }
        }


        public override async Task<AddCommentResponse> AddComment(AddCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var postCollection = database.GetCollection<Post>("posts");
                var commentCollection = database.GetCollection<Comment>("comments");

                // Create a new child comment
                var childComment = new Comment(request.Postid, "", request.Userid, request.Content);

                // Insert the child comment into the comments collection
                await commentCollection.InsertOneAsync(childComment);

                // Generate a new child comment ID
                var childCommentId = childComment.CommentId;

                // Find the post by post ID
                var postFilter = Builders<Post>.Filter.Eq(p => p.PostId, request.Postid);
                var post = await postCollection.Find(postFilter).FirstOrDefaultAsync();

                if (post != null)
                {
                    // Add the new child comment ID to the post's ChildCommentIds list
                    post.ChildCommentIds.Add(childCommentId);

                    // Update the post with the new ChildCommentIds
                    var updatePost = Builders<Post>.Update.Set(p => p.ChildCommentIds, post.ChildCommentIds);
                    var updateResultPost = await postCollection.UpdateOneAsync(postFilter, updatePost);

                    if (updateResultPost.ModifiedCount == 1)
                    {
                        return new AddCommentResponse { Message = "Child comment added successfully." };
                    } else
                    {
                        return new AddCommentResponse { Message = "Failed to add child comment to the post." };
                    }
                } else
                {
                    return new AddCommentResponse { Message = "Post not found." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a child comment to a post.");
                return new AddCommentResponse { Message = "An error occurred while adding the child comment." };
            }
        }


        public override async Task<DeleteCommentResponse> DeleteComment(DeleteCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var collection = database.GetCollection<Comment>("comments");

                // Find the comment to be deleted
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, request.Commentid);
                var comment = await collection.Find(filter).FirstOrDefaultAsync();

                if (comment == null)
                {
                    return new DeleteCommentResponse { Message = "Comment not found." };
                }

                // Recursively delete children comments and update parent comment's ChildCommentIds
                await DeleteCommentAndChildren(collection, comment);

                return new DeleteCommentResponse { Message = "Comment and its children deleted successfully." };
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting a comment.");
                return new DeleteCommentResponse { Message = "An error occurred while deleting the comment." };
            }
        }

        private async Task DeleteCommentAndChildren(IMongoCollection<Comment> collection, Comment comment)
        {
            // Recursively delete children comments and update parent comment's ChildCommentIds
            foreach (var childCommentId in comment.ChildCommentIds)
            {
                var childFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, childCommentId);
                var childComment = await collection.Find(childFilter).FirstOrDefaultAsync();

                if (childComment != null)
                {
                    await DeleteCommentAndChildren(collection, childComment);
                }
            }

            // Remove this comment's ID from the parent comment's ChildCommentIds
            if (!string.IsNullOrEmpty(comment.ParentCommentId))
            {
                var parentFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, comment.ParentCommentId);
                var update = Builders<Comment>.Update.Pull(c => c.ChildCommentIds, comment.CommentId);
                await collection.UpdateOneAsync(parentFilter, update);
            }

            // Finally, delete this comment
            var deleteFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, comment.CommentId);
            await collection.DeleteOneAsync(deleteFilter);
        }




        public override async Task<LikeCommentResponse> LikeComment(LikeCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var collection = database.GetCollection<Comment>("comments");

                // Find the comment to be liked
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, request.Commentid);

                // Update to add the user's ID to the UserLikesIds list in the comment
                var update = Builders<Comment>.Update.Push(c => c.UserIdLikes, request.Userid);

                var updateResult = await collection.UpdateOneAsync(filter, update);

                if (updateResult.ModifiedCount == 1)
                {
                    return new LikeCommentResponse { Message = "Comment liked successfully." };
                } else
                {
                    return new LikeCommentResponse { Message = "Comment not found or failed to like." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while liking a comment.");
                return new LikeCommentResponse { Message = "An error occurred while liking the comment." };
            }
        }



        public override async Task<AddCommentToCommentResponse> AddCommentToComment(AddCommentToCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var commentCollection = database.GetCollection<Comment>("comments");

                // Create a new Comment object for the child comment
                var childComment = new Comment(request.ParentPostId, request.Parentcommentid, request.Userid, request.Content);
                _logger.LogInformation($"Received request with content: {request.Content}");

                // Find the parent comment by comment ID
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, request.Parentcommentid);
                var update = Builders<Comment>.Update.Push(c => c.ChildCommentIds, childComment.CommentId);

                var updateResult = await commentCollection.UpdateOneAsync(filter, update);

                if (updateResult.ModifiedCount == 1)
                {
                    // Insert the child comment into the comments collection
                    await commentCollection.InsertOneAsync(childComment);

                    return new AddCommentToCommentResponse { Message = "Child comment added successfully." };
                } else
                {
                    return new AddCommentToCommentResponse { Message = "Parent comment not found or failed to add child comment." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a child comment to a parent comment.");
                return new AddCommentToCommentResponse { Message = "An error occurred while adding the child comment." };
            }
        }



        public override async Task<EditCommentResponse> EditComment(EditCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database and collection
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts");
                var commentCollection = database.GetCollection<Comment>("comments");

                // Find the comment by comment ID
                var filter = Builders<Comment>.Filter.Eq(c => c.CommentId, request.Commentid);

                // Define the update to set the new content value
                var update = Builders<Comment>.Update.Set(c => c.Content, request.Content);

                var updateResult = await commentCollection.UpdateOneAsync(filter, update);

                if (updateResult.ModifiedCount == 1)
                {
                    return new EditCommentResponse { Message = "Comment edited successfully." };
                } else
                {
                    return new EditCommentResponse { Message = "Comment not found or failed to edit." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing a comment.");
                return new EditCommentResponse { Message = "An error occurred while editing the comment." };
            }
        }



    }
}
