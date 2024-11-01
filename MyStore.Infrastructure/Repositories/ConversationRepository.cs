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

        public async Task AddMessageAsync(string id, string message, bool isUser)
        {
            var filter = Builders<Conversations>.Filter.Eq(m => m.Id, id);
            var update = Builders<Conversations>.Update.Push(m => m.Messages, new Message
            {
                Content = message,
                IsUser = isUser
            });
            await _conversationCollection.UpdateOneAsync(filter, update);
        }

        public async Task<Conversations?> FindConversationAsync(string id)
            => await _conversationCollection.Find(x => x.Id == id).FirstOrDefaultAsync(); 

        public async Task RemoveConversationAsync(string id) =>
            await _conversationCollection.DeleteOneAsync(x => x.Id == id);

        public async Task<IEnumerable<string?>> GetConversationIdsAsync()
            => await _conversationCollection
                .Find(_ => true)
                .Project(e => e.Id)
                .ToListAsync();
    }
}
