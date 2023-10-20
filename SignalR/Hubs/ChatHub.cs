using Microsoft.AspNetCore.SignalR;
using SignalR.Services;

namespace SignalR.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessagingServiceClientWrapper _messagingService;

        public ChatHub(MessagingServiceClientWrapper messagingService)
        {
            _messagingService = messagingService;
        }

        public async Task SendMessage(string sender, string receiver, string message)
        {
            try
            {
                var response = _messagingService.SendMessage(sender, receiver, message);

                // Depending on your requirements, you may broadcast to all, or just to specific clients.
                await Clients.All.SendAsync("ReceiveMessage", sender, message);

                // For private messaging, you could use something like:
                // await Clients.User(receiver).SendAsync("ReceiveMessage", sender, message);
            } catch (Grpc.Core.RpcException ex)
            {
                Console.WriteLine($"gRPC Error: {ex.Status.Detail}");
            }
        }
    }
}
