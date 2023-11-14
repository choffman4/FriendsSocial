using Grpc.Core;
using GrpcMongoMessagingService;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FriendsMudBlazorApp.Services
{
    public class MessagingServiceClientWrapper
    {
        private readonly MongoMessagingService.MongoMessagingServiceClient _client;
        private readonly ILogger<MessagingServiceClientWrapper> _logger;

        public MessagingServiceClientWrapper(MongoMessagingService.MongoMessagingServiceClient client, ILogger<MessagingServiceClientWrapper> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<SendMessageResponse> SendMessageAsync(string senderId, string receiverId, string content)
        {
            var request = new SendMessageRequest
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content
            };

            try
            {
                return await _client.SendMessageAsync(request);
            } catch (RpcException ex)
            {
                _logger.LogError(ex, "Error sending message via gRPC.");
                throw;
            }
        }

        public async Task<List<Message>> RetrieveMessagesAsync(string userId1, string userId2, string lastMessageId = null, string lastTimestamp = null, int pageSize = 10)
        {
            var request = new RetrieveMessagesRequest
            {
                UserId1 = userId1,
                UserId2 = userId2,
                LastMessageId = lastMessageId ?? string.Empty,
                LastTimestamp = lastTimestamp ?? string.Empty,
                PageSize = pageSize
            };

            try
            {
                var response = await _client.RetrieveMessagesAsync(request);
                return response.Messages.ToList();
            } catch (RpcException ex)
            {
                _logger.LogError(ex, "Error retrieving messages via gRPC.");
                throw;
            }
        }

        // Implement other methods (MarkMessageAsRead, DeleteMessage, etc.) in a similar fashion
    }
}
