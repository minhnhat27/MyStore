using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Presentation.Hubs.ConnectionManager;
using System.Security.Claims;

namespace MyStore.Presentation.Hubs
{
    public class ConversationIdResponse
    {
        public string? Id { get; set; }
        public int Unread { get; set; }
        public bool Closed { get; set; }
    }

    public class ChatBox(IConversationRepository conversationRepository, IConnectionManager connectionManager) : Hub
    {
        private readonly IConversationRepository _conversationRepository = conversationRepository;
        private readonly IConnectionManager _connectionManager = connectionManager;
       
        public bool GetAdminOnline()
                    => _connectionManager.AdminCount > 0;

        public async Task SendToAdmin(string session, string message, string? image) {
            await Clients.Group("AdminGroup").SendAsync("onAdmin", session, message, image);
            await _conversationRepository.AddMessageAsync(session, message, true, image);
            await _conversationRepository.UpdateUnread(session, false, 1);
        }

        [Authorize(Roles = "Admin")]
        public async Task SendToUser(string session, string message, string? image) {
            if(_connectionManager.TryGetConnectionIdByConversationId(session, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("onUser", message, image);
            }
            await _conversationRepository.AddMessageAsync(session, message, false, image);
            await _conversationRepository.UpdateUnread(session, true, 1);

        }

        [Authorize(Roles = "Admin")]
        public async Task<IEnumerable<ConversationIdResponse>> GetConversations()
            => (await _conversationRepository.GetConversationIdsAsync())
            .Select(e => new ConversationIdResponse
            {
                Id = e.Id,
                Closed = e.Closed,
                Unread = e.Unread.Admin,
            });

        public async Task<string> StartChat()
        {
            string session = ObjectId.GenerateNewId().ToString();
            UpdateConnectionId(session);
            await _conversationRepository.CreateConversationAsync(session);
            return session;
        }
        
        public async Task<ConversationDTO?> GetConversation(string session)
        {
            var conversation = await _conversationRepository.FindConversationAsync(session);
            if (conversation == null)
            {
                return null;
            }
            var isUser = !Context.User?.FindAll(ClaimTypes.Role).Select(e => e.Value).Contains("Admin") ?? true;
            await _conversationRepository.UpdateUnread(session, isUser);
            return new ConversationDTO
            {
                Id = conversation.Id,
                Unread = conversation.Unread.User,
                Messages = conversation.Messages
            };
        }

        [Authorize(Roles = "Admin")]
        public async Task RemoveChat(string session)
        {
            await _conversationRepository.RemoveConversationAsync(session);
            if (_connectionManager.TryGetConnectionIdByConversationId(session, out string connectionId))
            {
                await Clients.Client(connectionId).SendAsync("CLOSE_CHAT", "CLOSE_CHAT");
                _connectionManager.TryRemoveUserConnection(session);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<int> TotalUnread()
            => await _conversationRepository.GetAdminUnreadAsync();

        public async Task CloseChat(string session)
        {
            await Clients.Group("AdminGroup").SendAsync("CLOSE_CHAT", session);
            await _conversationRepository.CloseChat(session);
            _connectionManager.TryRemoveUserConnection(session);
        }

        public void UpdateConnectionId(string session)
            => _connectionManager.TryAddOrUpdateUserConnection(session, Context.ConnectionId);

        public async Task<int> GetUnread(string session)
            => await _conversationRepository.GetUnreadAsync(session);

        public async Task ReadMessage(string session)
            => await _conversationRepository.UpdateUnread(session, true);

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
