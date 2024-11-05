using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class ConversationRepository : IConversationRepository
    {
        private readonly IMongoCollection<Conversations> _conversationCollection;
        public ConversationRepository(IOptions<ConversationDbSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);
            _conversationCollection = mongoDatabase.GetCollection<Conversations>(options.Value.CollectionName);
        }

        public async Task<IEnumerable<Conversations>> GetConversationsAsync() => await _conversationCollection.Find(_ => true).ToListAsync();

        public async Task CreateConversationAsync(string id)
        {
            var conversation = new Conversations { Id = id };
            await _conversationCollection.InsertOneAsync(conversation);
        }

        public async Task AddMessageAsync(string id, string message, bool isUser, string? image)
        {
            var filter = Builders<Conversations>.Filter.Eq(m => m.Id, id);
            var update = Builders<Conversations>.Update.Push(m => m.Messages, new Message
            {
                Content = message,
                IsUser = isUser,
                Image = image
            });
            await _conversationCollection.UpdateOneAsync(filter, update);
        }

        public async Task<Conversations?> FindConversationAsync(string id)
            => await _conversationCollection.Find(x => x.Id == id).FirstOrDefaultAsync(); 

        public async Task RemoveConversationAsync(string id) =>
            await _conversationCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<IEnumerable<Conversations>> GetConversationIdsAsync()
            => await _conversationCollection
                .Find(_ => true)
                .Project(e => new Conversations
                {
                    Id = e.Id,
                    Closed = e.Closed,
                    Unread = e.Unread,
                    Messages = new()
                })
                .ToListAsync();

        public async Task CloseChat(string id)
            => await _conversationCollection
                .UpdateOneAsync(e => e.Id == id, Builders<Conversations>.Update.Set(e => e.Closed, true));

        public async Task UpdateUnread(string id, bool isUser, int add)
        {
            UpdateDefinition<Conversations> update;
            if (add == 0)
            {
                update = isUser
                   ? Builders<Conversations>.Update.Set(e => e.Unread.User, add)
                   : Builders<Conversations>.Update.Set(e => e.Unread.Admin, add);
            }
            else
            {
                update = isUser
                    ? Builders<Conversations>.Update.Inc(e => e.Unread.User, add)
                    : Builders<Conversations>.Update.Inc(e => e.Unread.Admin, add);
            }

            await _conversationCollection.UpdateOneAsync(e => e.Id == id, update);
        }

        public async Task<int> GetUnreadAsync(string id, bool isUser = true)
        {
            var conversation = await _conversationCollection
                .Find(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                return 0;
            }

            return isUser ? conversation.Unread.User : conversation.Unread.Admin;
        }
    }
}
