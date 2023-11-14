using Grpc.Core;
using MongoDB.Driver;
using System.Reflection;
using GrpcMongoMessagingService.MessageDocument;
using MongoDB.Bson;

namespace GrpcMongoMessagingService.Services
{
    public class MessagingGrpcService : MongoMessagingService.MongoMessagingServiceBase
    {
        private readonly IMongoCollection<MessageObject> _messages;

        public MessagingGrpcService(MongoClientSettings settings)
        {
            var client = new MongoClient(settings);
            var database = client.GetDatabase("messagesDB");
            _messages = database.GetCollection<MessageObject>("messages");
        }

        public override async Task<SendMessageResponse> SendMessage(SendMessageRequest request, ServerCallContext context)
        {
            try
            {
                // Create a new message using the data from the request.
                var newMessage = new MessageObject
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    Content = request.Content,
                    Timestamp = DateTime.UtcNow,
                    IsRead = false // By default, the message is unread.
                };

                // Insert the message into MongoDB.
                await _messages.InsertOneAsync(newMessage);

                // Return a response with the ID of the inserted message.
                return new SendMessageResponse
                {
                    MessageId = newMessage.Id,
                    Status = "Success"
                };
            } catch (Exception ex) // It's a good idea to catch potential exceptions for better error handling.
            {
                // Log the exception here if you have logging set up.
                return new SendMessageResponse
                {
                    MessageId = string.Empty, // No ID, because the operation failed.
                    Status = $"Failed: {ex.Message}"
                };
            }
        }

        public override async Task<RetrieveMessagesResponse> RetrieveMessages(RetrieveMessagesRequest request, ServerCallContext context)
        {
            try
            {
                // Construct the filter for the message query
                var builder = Builders<MessageObject>.Filter;
                var filter = builder.Or(
                    builder.And(builder.Eq(m => m.SenderId, request.UserId1), builder.Eq(m => m.ReceiverId, request.UserId2)),
                    builder.And(builder.Eq(m => m.SenderId, request.UserId2), builder.Eq(m => m.ReceiverId, request.UserId1))
                );

                if (!string.IsNullOrEmpty(request.LastMessageId))
                {
                    var lastTimestamp = DateTime.Parse(request.LastTimestamp); // Assumes timestamp is in a parseable format
                    var objectId = new ObjectId(request.LastMessageId);

                    // Modify the filter to fetch messages older than the last loaded message
                    filter &= builder.Lt(m => m.Timestamp, lastTimestamp) & builder.Lt(m => m.Id, objectId.ToString());
                }

                // Sort the messages in descending order of timestamp and limit the result
                var messagesFromDb = await _messages.Find(filter)
                                                    .SortByDescending(m => m.Timestamp)
                                                    .ThenByDescending(m => m.Id)
                                                    .Limit(request.PageSize)
                                                    .ToListAsync();

                // Convert MongoDB messages to gRPC messages
                var grpcMessages = messagesFromDb.Select(m => new Message
                {
                    MessageId = m.Id.ToString(),
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp.ToString(),
                    IsRead = m.IsRead
                }).ToList();

                return new RetrieveMessagesResponse { Messages = { grpcMessages } };
            } catch (Exception ex)
            {
                // Handle exceptions and logging
                return new RetrieveMessagesResponse();
            }
        }


        public override async Task<RetrieveUnreadMessagesResponse> RetrieveUnreadMessages(RetrieveUnreadMessagesRequest request, ServerCallContext context)
        {
            try
            {
                // Define the filter criteria
                var filter = Builders<MessageObject>.Filter.And(
                    Builders<MessageObject>.Filter.Eq(m => m.ReceiverId, request.UserId),
                    Builders<MessageObject>.Filter.Eq(m => m.SenderId, request.FriendId),
                    Builders<MessageObject>.Filter.Eq(m => m.IsRead, false)
                );

                // Fetch the unread messages from MongoDB
                var unreadMessages = await _messages.Find(filter).ToListAsync();

                // Convert MongoDB messages to gRPC messages for the response
                var grpcMessages = unreadMessages.Select(m => new Message
                {
                    MessageId = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Timestamp = m.Timestamp.ToString(),
                    IsRead = m.IsRead
                }).ToList();

                // Return the response with the list of unread messages
                return new RetrieveUnreadMessagesResponse
                {
                    Messages = { grpcMessages }
                };
            } catch (Exception ex) // Catch any exceptions that might occur
            {
                // Log the exception (if you have logging set up)
                // Return an empty response (or customize the response based on your application's error-handling strategy)
                return new RetrieveUnreadMessagesResponse();
            }
        }

        public override async Task<MarkMessageAsReadResponse> MarkMessageAsRead(MarkMessageAsReadRequest request, ServerCallContext context)
        {
            try
            {
                // Define the filter criteria: match any message whose ID is in the provided list
                var filter = Builders<MessageObject>.Filter.In(m => m.Id, request.MessageIds);

                // Define the update operation: set the IsRead property to true
                var update = Builders<MessageObject>.Update.Set(m => m.IsRead, true);

                // Update the messages in MongoDB
                var updateResult = await _messages.UpdateManyAsync(filter, update);

                // If no messages were updated, return a "No messages updated" status
                if (updateResult.MatchedCount == 0)
                {
                    return new MarkMessageAsReadResponse
                    {
                        Status = "No messages updated"
                    };
                }

                // Return a success status
                return new MarkMessageAsReadResponse
                {
                    Status = "Success"
                };
            } catch (Exception ex) // Catch any exceptions that might occur
            {
                // Log the exception (if you have logging set up)
                // Return a failure status with the exception message
                return new MarkMessageAsReadResponse
                {
                    Status = $"Failed: {ex.Message}"
                };
            }
        }

        public override async Task<DeleteMessageResponse> DeleteMessage(DeleteMessageRequest request, ServerCallContext context)
        {
            try
            {
                // Define the filter criteria: match the message whose ID is provided
                var filter = Builders<MessageObject>.Filter.Eq(m => m.Id, request.MessageId);

                // Delete the message from MongoDB
                var deleteResult = await _messages.DeleteOneAsync(filter);

                // If no messages were deleted, return a "Message not found" status
                if (deleteResult.DeletedCount == 0)
                {
                    return new DeleteMessageResponse
                    {
                        Status = "Message not found"
                    };
                }

                // Return a success status
                return new DeleteMessageResponse
                {
                    Status = "Success"
                };
            } catch (Exception ex) // Catch any exceptions that might occur
            {
                // Log the exception (if you have logging set up)
                // Return a failure status with the exception message
                return new DeleteMessageResponse
                {
                    Status = $"Failed: {ex.Message}"
                };
            }
        }

        public override async Task<EditMessageResponse> EditMessage(EditMessageRequest request, ServerCallContext context)
        {
            try
            {
                // Define the filter criteria: match the message whose ID is provided
                var filter = Builders<MessageObject>.Filter.Eq(m => m.Id, request.MessageId);

                // Define the update operation: set the content to the new content provided
                var update = Builders<MessageObject>.Update.Set(m => m.Content, request.NewContent);

                // Update the message in MongoDB
                var updateResult = await _messages.UpdateOneAsync(filter, update);

                // If no messages were updated, return a "Message not found" status
                if (updateResult.ModifiedCount == 0)
                {
                    return new EditMessageResponse
                    {
                        Status = "Message not found",
                        EditedMessage = null
                    };
                }

                // Retrieve the updated message to return it in the response
                var editedMessage = await _messages.Find(filter).FirstOrDefaultAsync();

                // Return a success status and the updated message
                return new EditMessageResponse
                {
                    Status = "Success",
                    EditedMessage = new GrpcMongoMessagingService.Message
                    {
                        MessageId = editedMessage.Id,
                        SenderId = editedMessage.SenderId,
                        ReceiverId = editedMessage.ReceiverId,
                        Content = editedMessage.Content,
                        Timestamp = editedMessage.Timestamp.ToString(),
                        IsRead = editedMessage.IsRead
                    }
                };
            } catch (Exception ex) // Catch any exceptions that might occur
            {
                // Log the exception (if you have logging set up)
                // Return a failure status with the exception message
                return new EditMessageResponse
                {
                    Status = $"Failed: {ex.Message}",
                    EditedMessage = null
                };
            }
        }
    }
}
