using MyStore.Domain.Entities;

namespace MyStore.Application.IRepositories
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notifications>> GetNotificationsAsync();
    }
}
