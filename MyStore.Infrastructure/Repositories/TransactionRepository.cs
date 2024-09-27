using Microsoft.EntityFrameworkCore.Storage;
using MyStore.Application.IRepositories;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class TransactionRepository(MyDbContext dbcontext) : ITransactionRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _dbContext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _dbContext.Database.RollbackTransactionAsync();
        }
    }
}
