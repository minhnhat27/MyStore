using Microsoft.EntityFrameworkCore.Storage;

namespace MyStore.Application.IRepositories
{
    public interface ITransactionRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
