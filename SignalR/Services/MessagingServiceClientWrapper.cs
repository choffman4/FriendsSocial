using GrpcMongoMessagingService;
using Microsoft.Extensions.Logging;

namespace SignalR.Services
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

        // Here, you can create methods that abstract away the details of using the gRPC client.
        // For example:

        public SendMessageResponse SendMessage(string senderId, string receiverId, string content)
        {
            try
            {
                _logger.LogInformation($"Sending message from {senderId} to {receiverId}.");

                var request = new SendMessageRequest
                {
                    SenderId = senderId,
                    ReceiverId = receiverId,
                    Content = content
                };

                var response = _client.SendMessage(request);

                _logger.LogInformation($"Message sent successfully.");

                return response;
            } catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message via gRPC.");
                throw;  // You can decide whether you want to re-throw the exception or handle it differently.
            }
        }

        // Method for retrieving messages
        public RetrieveMessagesResponse RetrieveMessages(string userId1, string userId2)
        {
            var request = new RetrieveMessagesRequest
            {
                UserId1 = userId1,
                UserId2 = userId2
            };

            return _client.RetrieveMessages(request);
        }

        // Method for marking messages as read
        public MarkMessageAsReadResponse MarkMessageAsRead(IEnumerable<string> messageIds)
        {
            var request = new MarkMessageAsReadRequest();
            request.MessageIds.AddRange(messageIds);

            return _client.MarkMessageAsRead(request);
        }

        // Method for deleting messages
        public DeleteMessageResponse DeleteMessage(string messageId)
        {
            var request = new DeleteMessageRequest
            {
                MessageId = messageId
            };

            return _client.DeleteMessage(request);
        }

        // Method for editing messages
        public EditMessageResponse EditMessage(string messageId, string newContent)
        {
            var request = new EditMessageRequest
            {
                MessageId = messageId,
                NewContent = newContent
            };

            return _client.EditMessage(request);
        }
    }
}
