﻿using Grpc.Core;
using MongoDB.Driver;
using GrpcMongoPostingService.PostProperties;
using MongoDB.Bson;
using System.Globalization;

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

                //var profileDatabase = mongoClient.GetDatabase("profileDatabase");
                //var profileCollection = profileDatabase.GetCollection<Profile>("profiles");

                //var username = await profileCollection.Find(p => p.UserId == request.Userid).Project(p => p.Username).FirstOrDefaultAsync();

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
                        ChildCommentIds = { }, // Initialize the repeated field
                        Likes = { }
                        //Username = username
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

                    // Loop through post.Likes and add each like to the list of likes in the response
                    foreach (var like in post.UserIdLikes)
                    {
                        // Add the like to the response
                        response.Likes.Add(like);
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
                var profilecollection = database.GetCollection<Profile>("profiles");
                var databasePosts = mongoClient.GetDatabase("profilePosts");
                var postCollection = databasePosts.GetCollection<Post>("posts");
                var commentCollection = databasePosts.GetCollection<Comment>("comments");

                // Find all posts by the user ID
                var postFilter = Builders<Post>.Filter.Eq(p => p.UserId, request.Userid);
                var postCursor = await postCollection.Find(postFilter).ToCursorAsync();

                var username = await profilecollection.Find(p => p.UserId == request.Userid).Project(p => p.Username).FirstOrDefaultAsync();

                // Iterate through the posts and stream each one
                foreach (var post in postCursor.ToEnumerable())
                {
                    // Convert the post to the response message
                    var response = new GetAllPostsByUserIdResponse
                    {
                        Userid = post.UserId,
                        Username = username,
                        Postid = post.PostId,
                        Title = post.Title,
                        Content = post.Content,
                        Date = post.PostedDate.ToString(), // Convert the date to string
                        ChildCommentIds = { }, // Initialize the repeated field
                        Likes = { },
                        FirstName = await profilecollection.Find(p => p.UserId == post.UserId).Project(p => p.FirstName).FirstOrDefaultAsync(),
                        LastName = await profilecollection.Find(p => p.UserId == post.UserId).Project(p => p.LastName).FirstOrDefaultAsync()
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

                    // Loop through post.Likes and add each like to the list of likes in the response
                    foreach (var like in post.UserIdLikes)
                    {
                        // Add the like to the response
                        response.Likes.Add(like);
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
                existingPost.LastEditedDate = DateTime.UtcNow;

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
                var childComment = new Comment(request.Postid, request.Postid, request.Userid, request.Content);

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

        public override async Task<GetCommentResponse> GetComment(GetCommentRequest request, ServerCallContext context)
        {
            try
            {
                // Connect to your MongoDB database
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var database = mongoClient.GetDatabase("profilePosts"); // Use the appropriate database name
                var commentCollection = database.GetCollection<Comment>("comments");

                // Find the comment by its CommentId
                var commentFilter = Builders<Comment>.Filter.Eq(c => c.CommentId, request.Commentid);
                var comment = await commentCollection.Find(commentFilter).FirstOrDefaultAsync();

                if (comment != null)
                {
                    // Convert the comment to the response message
                    var response = new GetCommentResponse
                    {
                        Userid = comment.UserId,
                        Content = comment.Content,
                        Date = comment.CommentedDate.ToString(),
                        ChildCommentIds = { }, // Initialize the repeated field
                        Likes = { } // Initialize the repeated field
                    };

                    // Loop through comment.ChildCommentIds and add each child comment ID to the list of child comment IDs in the response
                    foreach (var childCommentId in comment.ChildCommentIds)
                    {
                        // Add the child comment ID to the response
                        response.ChildCommentIds.Add(childCommentId);
                    }

                    // Loop through comment.Likes and add each like to the list of likes in the response
                    foreach (var like in comment.UserIdLikes)
                    {
                        // Add the like to the response
                        response.Likes.Add(like);
                    }

                    return response;
                } else
                {
                    return new GetCommentResponse { Message = "Comment not found." };
                }
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting a comment by ID.");
                return new GetCommentResponse { Message = "An error occurred while getting the comment." };
            }
        }


        public override async Task<GetPostsResponse> GetPosts(GetPostsRequest request, ServerCallContext context)
        {
            try
            {
                var mongoClient = new MongoClient(_configuration.GetConnectionString("MongoDb"));
                var profileFriendsDatabase = mongoClient.GetDatabase("profileFriends");
                var postsDatabase = mongoClient.GetDatabase("profilePosts");

                var friendshipsCollection = profileFriendsDatabase.GetCollection<Friendship>("friendships");
                var postCollection = postsDatabase.GetCollection<Post>("posts");

                var profileDatabase = mongoClient.GetDatabase("profileDatabase");
                var profileCollection = profileDatabase.GetCollection<Profile>("profiles");

                _logger.LogInformation("UserId: {UserId}", request.UserId);

                var friendFilter = Builders<Friendship>.Filter.Or(
                    Builders<Friendship>.Filter.Eq(f => f.friend1Id, request.UserId),
                    Builders<Friendship>.Filter.Eq(f => f.friend2Id, request.UserId)
                );

                var username = await profileCollection.Find(p => p.UserId == request.UserId).Project(p => p.Username).FirstOrDefaultAsync();

                var friendships = await friendshipsCollection.Find(friendFilter).ToListAsync();
                var friendIds = friendships.Select(f => f.friend1Id == request.UserId ? f.friend2Id : f.friend1Id).ToList();
                friendIds.Add(request.UserId);

                var postFilter = Builders<Post>.Filter.In(p => p.UserId, friendIds);

                // Use the lastPostId to filter posts if provided
                if (!string.IsNullOrEmpty(request.LastPostId))
                {
                    var lastPostObjectId = new ObjectId(request.LastPostId);
                    postFilter &= Builders<Post>.Filter.Lt(p => p.Id, lastPostObjectId);
                }

                // Fetch the posts with pagination
                var posts = await postCollection.Find(postFilter)
                                                .SortByDescending(p => p.PostedDate)
                                                .ThenByDescending(p => p.Id)
                                                .Limit(request.Limit)
                                                .ToListAsync();

                var response = new GetPostsResponse();
                foreach (var post in posts)
                {
                    var postResponse = new PostObject
                    {
                        PostId = post.PostId.ToString(),
                        UserId = post.UserId,
                        Username = await profileCollection.Find(p => p.UserId == post.UserId).Project(p => p.Username).FirstOrDefaultAsync(),
                        Title = post.Title,
                        Content = post.Content,
                        Date = post.PostedDate.ToString("o", CultureInfo.InvariantCulture),
                        ChildCommentIds = { post.ChildCommentIds },
                        Likes = { post.UserIdLikes.Select(id => id.ToString()) },
                        FirstName = await profileCollection.Find(p => p.UserId == post.UserId).Project(p => p.FirstName).FirstOrDefaultAsync(),
                        LastName = await profileCollection.Find(p => p.UserId == post.UserId).Project(p => p.LastName).FirstOrDefaultAsync()
                    };

                    response.Posts.Add(postResponse);
                }

                // Set the lastPostId to the ObjectId of the last post
                if (posts.Any())
                {
                    var lastPost = posts.Last();
                    response.LastPostId = lastPost.Id.ToString(); // Convert the ObjectId to a string
                    response.LastPostTimestamp = lastPost.PostedDate.ToString("o", CultureInfo.InvariantCulture);
                }

                return response;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching paginated posts.");
                return new GetPostsResponse
                {
                    Message = "Error occurred while fetching paginated posts."
                };
            }
        }

    }
}
