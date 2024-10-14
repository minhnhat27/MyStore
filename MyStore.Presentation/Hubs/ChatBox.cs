using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MyStore.Presentation.Hubs.Message;
using System.Collections.Concurrent;
using System.Security.Claims;
using static MyStore.Presentation.Hubs.Message.MessageManager;

namespace MyStore.Presentation.Hubs
{
    public class MessageResponse
    {
        public string ConnectionId { get; set; }
        public IEnumerable<MessageStruct> Messages { get; set; }
    }

    public class ChatBox(IMessageManager messageManager) : Hub
    {
        private readonly IMessageManager _messageManager = messageManager;

        public async Task SendToAdmin(string message) {
            _messageManager.TryAddMessage(Context.ConnectionId, message);
            await Clients.Group("AdminGroup").SendAsync("onAdmin", Context.ConnectionId, message);
        }
        public bool GetAdminOnline()
            => _messageManager.AdminCount > 0;

        [Authorize(Roles = "Admin")]
        public async Task SendToUser(string connectionId, string message) {
            _messageManager.TryAddMessage(connectionId, message, false);
            await Clients.Client(connectionId).SendAsync("onUser", message);
        }

        [Authorize(Roles = "Admin")]
        public IEnumerable<string> GetUserConnections()
            => _messageManager.TryGetAllConnection();

        [Authorize(Roles = "Admin")]
        public IEnumerable<MessageResponse> GetMessages()
            => _messageManager.GetMessages().Select(item => new MessageResponse
            {
                ConnectionId = item.Key,
                Messages = item.Value,
            });

        [Authorize(Roles = "Admin")]
        public async Task CloseChat(string connectionId)
        {
            _messageManager.StopChatting(connectionId);
            await Clients.Client(connectionId).SendAsync("CloseChat", "CLOSE_CHAT");
        }

        public override async Task OnConnectedAsync()
        {
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
            string adminGroup = "AdminGroup";

            if (roles != null && roles.Contains("Admin"))
            {
                _messageManager.TryAddAdmin(Context.ConnectionId);
                Console.WriteLine("Connected: " + Context.ConnectionId + " " + _messageManager.AdminCount);
                await Groups.AddToGroupAsync(Context.ConnectionId, adminGroup);
            }
            else
            {
                _messageManager.StartChatting(Context.ConnectionId);
                await Clients.Group(adminGroup).SendAsync("USER_CONNECT", Context.ConnectionId);
            }


            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
            string adminGroup = "AdminGroup";

            if (roles != null && roles.Contains("Admin"))
            {
                _messageManager.TryRemoveAdmin(Context.ConnectionId);
                Console.WriteLine("Disconnected: " + Context.ConnectionId + " " + _messageManager.AdminCount);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, adminGroup);
            }
            else
            {
                _messageManager.StopChatting(Context.ConnectionId);
                await Clients.Group(adminGroup).SendAsync("USER_DISCONNECT", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
