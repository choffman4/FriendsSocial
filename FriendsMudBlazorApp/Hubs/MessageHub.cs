using Microsoft.AspNetCore.SignalR;
using FriendsMudBlazorApp.Services;
using System.Threading.Tasks;

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

        public async Task SendMessage(string senderId, string receiverId, string groupId, string messageContent)
        {
            var saveResponse = await _messagingService.SendMessageAsync(senderId, receiverId, messageContent);

            Message message = new Message();
            message.SenderId = senderId;
            message.ReceiverId = receiverId;
            message.Content = messageContent;
            message.Timestamp = DateTime.UtcNow;

            if (saveResponse.Status == "Success")
            {
                // Send the message to the group
                await Clients.Group(groupId).SendAsync("ReceiveMessage", senderId, message);
            } else
            {
                _logger.LogWarning($"Failed to send message: {saveResponse.Status}");
            }
        }

        public async Task JoinGroup(string groupId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
            _logger.LogInformation($"User {Context.ConnectionId} joined group {groupId}");
        }

        public async Task LeaveGroup(string groupId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
            _logger.LogInformation($"User {Context.ConnectionId} left group {groupId}");
        }

        // Optional: Override OnConnectedAsync and OnDisconnectedAsync if you need to perform any specific logic when a user connects or disconnects
        public class Message
        {
            public string SenderId { get; set; }
            public string ReceiverId { get; set; }
            public string Content { get; set; }
            public DateTime Timestamp { get; set; }
            public bool IsRead { get; set; }
        }
    }
}
