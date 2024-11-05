using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversations>> GetConversationsAsync();
        Task<IEnumerable<Conversations>> GetConversationIdsAsync();
        Task<int> GetUnreadAsync(string id, bool isUser = true);
        Task CreateConversationAsync(string id);
        Task AddMessageAsync(string id, string message, bool isUser = true, string? image = null);
        Task<Conversations?> FindConversationAsync(string id);
        Task RemoveConversationAsync(string id);
        Task CloseChat(string id);
        Task UpdateUnread(string id, bool isUser, int add = 0);
    }
}
