using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class SizeRepository : Repository<Size>, ISizeRepository
    {
        public SizeRepository(MyDbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
