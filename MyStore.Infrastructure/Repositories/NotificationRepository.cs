using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notifications> _notificationCollection;
        public NotificationRepository(IOptions<MongoDbSettings> options)
        {
            var mongoClient = new MongoClient(options.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);
            _notificationCollection = mongoDatabase.GetCollection<Notifications>(options.Value.CollectionNotifications);
        }

        public async Task<IEnumerable<Notifications>> GetNotificationsAsync()
            => await _notificationCollection
                .Find(_ => true)
                .SortByDescending(notification => notification.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Notifications>> GetNotificationsAsync(int page, int pageSize)
            => await _notificationCollection
                .Find(_ => true)
                .SortByDescending(notification => notification.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

        public async Task<long> GetNotIsRead()
            => await _notificationCollection.Find(e => !e.IsRead).CountDocumentsAsync();

        public async Task AddNotificationAsync(Notifications notification)
            => await _notificationCollection.InsertOneAsync(notification);

        public async Task DeleteNotificationAsync(string id)
            => await _notificationCollection.DeleteOneAsync(x => x.Id == id && x.IsRead);

        public async Task DeleteNotIsReadNotificationAsync()
            => await _notificationCollection.DeleteManyAsync(x => x.IsRead);

        public async Task UpdateIsReadAsync(string id)
        {
            var update = Builders<Notifications>.Update.Set(e => e.IsRead, true);
            await _notificationCollection.UpdateOneAsync(e => e.Id == id, update);
        }
        public async Task UpdateAllIsReadAsync()
        {
            var update = Builders<Notifications>.Update.Set(n => n.IsRead, true);
            await _notificationCollection.UpdateManyAsync(e => !e.IsRead, update);
        }
    }
}
