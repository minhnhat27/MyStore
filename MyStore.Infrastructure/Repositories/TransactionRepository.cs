using Microsoft.EntityFrameworkCore.Storage;
using MyStore.Application.IRepository;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationContext _Dbcontext;
        public TransactionRepository(ApplicationContext dbcontext)
        {
            _Dbcontext = dbcontext;
        }
        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _Dbcontext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            await _Dbcontext.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            await _Dbcontext.Database.RollbackTransactionAsync();
        }
    }
}
