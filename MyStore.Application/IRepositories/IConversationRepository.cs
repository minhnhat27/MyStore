using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface IConversationRepository
    {
        Task<IEnumerable<Conversations>> GetConversationsAsync();
        Task<IEnumerable<string?>> GetConversationIdsAsync();
        Task CreateConversationAsync(string id);
        Task AddMessageAsync(string id, string message, bool isUser = true);
        Task<Conversations?> FindConversationAsync(string id);
        Task RemoveConversationAsync(string id);
    }
}
