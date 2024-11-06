
namespace MyStore.Presentation.Hubs.ConnectionManager
{
    public interface IConnectionManager
    {
        int AdminCount { get; }
        bool TryAddAdmin(string key);
        bool TryRemoveAdmin(string key);
        string TryAddOrUpdateUserConnection(string conversationId, string connectionId);
        bool TryRemoveUserConnection(string key);
        bool TryGetConnectionIdByConversationId(string conversationId, out string connectionId);
    }
}
