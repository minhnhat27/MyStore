using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class SizeRepository(MyDbContext dbcontext) : Repository<Size>(dbcontext), ISizeRepository
    {
    }
}
