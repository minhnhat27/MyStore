namespace MyStore.Infrastructure.DbContext
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionConversations { get; set; }
        public string CollectionNotifications { get; set; }
    }
}
