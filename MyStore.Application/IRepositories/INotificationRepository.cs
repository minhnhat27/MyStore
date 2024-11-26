using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notifications>> GetNotificationsAsync(int page, int pageSize);
        Task<long> GetNotIsRead();
        Task AddNotificationAsync(Notifications notification);
        Task DeleteNotificationAsync(string id);
        Task DeleteNotIsReadNotificationAsync();
        Task UpdateIsReadAsync(string id);
        Task UpdateAllIsReadAsync();
    }
}
