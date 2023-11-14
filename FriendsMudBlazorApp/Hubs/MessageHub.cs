using Microsoft.AspNetCore.SignalR;
using FriendsMudBlazorApp.Services;

namespace FriendsMudBlazorApp.Hubs
{
    public class MessageHub : Hub
    {
        private readonly MessagingServiceClientWrapper _messagingService;
        private readonly ILogger<MessageHub> _logger;

        public MessageHub(MessagingServiceClientWrapper messagingService, ILogger<MessageHub> logger)
        {
            _messagingService = messagingService;
            _logger = logger;
        }

        public async Task SendMessage(string senderId, string receiverId, string messageContent)
        {
            _logger.LogInformation($"Attempting to send message from {senderId} to {receiverId}");

            var saveResponse = await _messagingService.SendMessageAsync(senderId, receiverId, messageContent);

            _logger.LogInformation($"Message save response: {saveResponse.Status}");

            if (saveResponse.Status == "Success")
            {
                var message = new { /* ... */ };
                _logger.LogInformation($"Sending to {senderId}");
                await Clients.User(senderId).SendAsync("ReceiveMessage", senderId, message);

                _logger.LogInformation($"Sending to {receiverId}");
                await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            } else
            {
                _logger.LogWarning($"Failed to send message: {saveResponse.Status}");
            }
        }
    }
}
