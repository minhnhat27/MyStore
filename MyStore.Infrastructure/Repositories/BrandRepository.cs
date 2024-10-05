using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class BrandRepository(MyDbContext dbcontext) : Repository<Brand>(dbcontext), IBrandRepository
    {
    }
}
