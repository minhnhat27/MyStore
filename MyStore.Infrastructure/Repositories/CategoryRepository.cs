using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class CategoryRepository(MyDbContext dbcontext) : Repository<Category>(dbcontext), ICategoryRepository
    {
    }
}
