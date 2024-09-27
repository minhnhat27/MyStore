using Microsoft.EntityFrameworkCore;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.DbContextService
{
    public class DbContextService(IDbContextFactory<MyDbContext> dbContextFactory) : IDbContextService
    {
        private readonly IDbContextFactory<MyDbContext> _dbContextFactory = dbContextFactory;

        public async Task<MyDbContext> CreateDbContext()
        {
            return await _dbContextFactory.CreateDbContextAsync();
        }
    }
}
