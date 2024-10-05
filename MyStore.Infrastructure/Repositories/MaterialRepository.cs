using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class MaterialRepository(MyDbContext dbcontext) : Repository<Material>(dbcontext), IMaterialRepository
    {
    }
}
