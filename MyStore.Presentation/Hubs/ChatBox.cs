using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Presentation.Hubs.ConnectionManager;
using System.Security.Claims;

namespace MyStore.Presentation.Hubs
{
    public class ChatBox(IConversationRepository conversationRepository, IConnectionManager connectionManager) : Hub
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IConnectionManager _connectionManager = connectionManager;
       
        public bool GetAdminOnline()
                    => _connectionManager.AdminCount > 0;

        public async Task SendToAdmin(string session, string message) {
            await _conversationRepository.AddMessageAsync(session, message);
            await Clients.Group("AdminGroup").SendAsync("onAdmin", session, message);
        }

        [Authorize(Roles = "Admin")]
        public async Task SendToUser(string session, string message) {
            await _conversationRepository.AddMessageAsync(session, message, false);
            if(_connectionManager.TryGetConnectionIdByConversationId(session, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("onUser", message);
            }

        }

        [Authorize(Roles = "Admin")]
        public async Task<IEnumerable<string?>> GetConversations()
            => await _conversationRepository.GetConversationIdsAsync();

        public async Task<string> StartChat()
        {
            string adminGroup = "AdminGroup";
            string session = ObjectId.GenerateNewId().ToString();
            UpdateConnectionId(session);

            await _conversationRepository.CreateConversationAsync(session);
            await Clients.Group(adminGroup).SendAsync("USER_START_CHAT", session);
            return session;
        }
        
        public async Task<ConversationDTO?> GetConversation(string session)
        {
            var conversation = await _conversationRepository.FindConversationAsync(session);
            if (conversation == null)
            {
                return null;
            }
            return new ConversationDTO
            {
                Id = conversation.Id,
                Messages = conversation.Messages
            };
        }

        [Authorize(Roles = "Admin")]
        public async Task CloseChat(string session)
        {
            await _conversationRepository.RemoveConversationAsync(session);
            if (_connectionManager.TryGetConnectionIdByConversationId(session, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CLOSE_CHAT", "CLOSE_CHAT");
            }
        }

        public void UpdateConnectionId(string session)
            => _connectionManager.TryAddOrUpdateUserConnection(session, Context.ConnectionId);

        public override async Task OnConnectedAsync()
        {
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
            string adminGroup = "AdminGroup";
            if (roles != null && roles.Contains("Admin"))
            {
                _connectionManager.TryAddAdmin(Context.ConnectionId);
                await Groups.AddToGroupAsync(Context.ConnectionId, adminGroup);
            }

            //else
            //{
            //    //await _conversationRepository.CreateConversationAsync(Context.ConnectionId);
            //    //await Clients.Group(adminGroup).SendAsync("USER_CONNECT", Context.ConnectionId);
            //}
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value);
            string adminGroup = "AdminGroup";
            if (roles != null && roles.Contains("Admin"))
            {
                _connectionManager.TryRemoveAdmin(Context.ConnectionId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, adminGroup);
            }

            //else
            //{
            //    //_messageManager.StopChatting(Context.ConnectionId);
            //    await Clients.Group(adminGroup).SendAsync("USER_DISCONNECT", Context.ConnectionId);
            //}

            await base.OnDisconnectedAsync(exception);
        }
    }
}
