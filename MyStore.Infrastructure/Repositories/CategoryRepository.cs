using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(MyDbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
