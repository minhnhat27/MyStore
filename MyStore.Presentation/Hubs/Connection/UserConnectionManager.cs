using System.Collections.Concurrent;

namespace MyStore.Presentation.Hubs.Connection
{
    public class UserConnectionManager : IUserConnectionManager
    {
        private readonly ConcurrentDictionary<string, string> _userConnections = new();

        public int Connections => _userConnections.Count;

        public bool TryAddConnection(string userId, string connectionId)
            => _userConnections.TryAdd(userId, connectionId);

        public bool TryRemoveConnection(string userId, out string connectionId)
            => _userConnections.TryRemove(userId, out connectionId!);

        public bool TryGetConnection(string userId, out string connectionId)
            => _userConnections.TryGetValue(userId, out connectionId!);
    }
}
