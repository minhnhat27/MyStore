namespace MyStore.Presentation.Hubs.Connection
{
    public interface IUserConnectionManager
    {
        int Connections { get; }
        bool TryAddConnection(string userId, string connectionId);
        bool TryRemoveConnection(string userId, out string connectionId);
        bool TryGetConnection(string userId, out string connectionId);
    }
}
