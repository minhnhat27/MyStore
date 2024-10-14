using System;
using System.Collections.Concurrent;

namespace MyStore.Presentation.Hubs.Message
{
    public class MessageManager : IMessageManager
    {
        public struct MessageStruct(string message, bool isUser)
        {
            public string Message { get; } = message;
            public bool IsUser { get; } = isUser;
            public DateTime DateTime { get; } = DateTime.Now;
        }

        private readonly ConcurrentDictionary<string, IList<MessageStruct>> _messageManager = new();
        private readonly ConcurrentDictionary<string, bool> _adminConnections = new();

        public int UserCount => _messageManager.Count;
        public int AdminCount => _adminConnections.Count;

        public IEnumerable<string> GetAdminConnection()
            => _adminConnections.Select(e => e.Key);

        public bool TryAddAdmin(string key)
            => _adminConnections.TryAdd(key, true);
        public bool TryRemoveAdmin(string key)
            => _adminConnections.TryRemove(key, out _);

        public bool StartChatting(string key)
            => !_messageManager.TryGetValue(key, out var _) && _messageManager.TryAdd(key, []);

        public bool TryAddMessage(string key, string message, bool isUser)
        {
            var newMessage = new MessageStruct(message, isUser);
            if (_messageManager.TryGetValue(key, out var messageList))
            {
                messageList.Add(newMessage);
                return true;
            }
            else
            {
                return _messageManager.TryAdd(key, [newMessage]);
            }
        }

        public bool TryGetMessages(string key, out IList<MessageStruct> messages)
        {
            return _messageManager.TryGetValue(key, out messages);
        }
        public IEnumerable<string> TryGetAllConnection()
            => _messageManager.Select(e => e.Key);


        public bool StopChatting(string key)
            => _messageManager.TryRemove(key, out _);

        public Dictionary<string, IList<MessageStruct>> GetMessages()
            => _messageManager.ToDictionary();
    }
}
