using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.DbContextService
{
    public interface IDbContextService
    {
        Task<MyDbContext> CreateDbContext();
    }
}
