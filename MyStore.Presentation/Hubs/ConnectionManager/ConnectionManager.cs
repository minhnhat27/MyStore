using System.Collections.Concurrent;

namespace MyStore.Presentation.Hubs.ConnectionManager
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, bool> _adminConnections = new();
        private readonly ConcurrentDictionary<string, string> _userConnections = new();
        public int AdminCount => _adminConnections.Count;

        public bool TryAddAdmin(string key)
            => _adminConnections.TryAdd(key, true);
        public bool TryRemoveAdmin(string key)
            => _adminConnections.TryRemove(key, out _);

        public string TryAddOrUpdateUserConnection(string conversationId, string connectionId)
            => _userConnections.AddOrUpdate(conversationId, connectionId, (key, oldValue) => connectionId);

        public bool TryRemoveUserConnection(string key)
            => _userConnections.TryRemove(key, out _);

        public bool TryGetConnectionIdByConversationId(string conversationId, out string connectionId)
            => _userConnections.TryGetValue(conversationId, out connectionId);
    }
}
