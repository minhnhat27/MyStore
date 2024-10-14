using static MyStore.Presentation.Hubs.Message.MessageManager;

namespace MyStore.Presentation.Hubs.Message
{
    public interface IMessageManager
    {
        int UserCount { get; }
        int AdminCount { get; }
        bool StartChatting(string key);
        bool TryAddMessage(string key, string message, bool isUser = true);
        bool TryGetMessages(string key, out IList<MessageStruct> messages);
        Dictionary<string, IList<MessageStruct>> GetMessages();
        IEnumerable<string> TryGetAllConnection();
        bool StopChatting(string key);

        IEnumerable<string> GetAdminConnection();
        bool TryAddAdmin(string key);
        bool TryRemoveAdmin(string key);
    }
}
