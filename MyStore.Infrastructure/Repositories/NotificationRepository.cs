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

        public async Task<IEnumerable<Notifications>> GetNotificationsAsync() =>
            await _notificationCollection.Find(_ => true).ToListAsync();

    }
}
