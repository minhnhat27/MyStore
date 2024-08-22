using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;

namespace MyStore.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly MyDbContext _dbContext;
        public UserRepository(MyDbContext dbcontext) : base(dbcontext) => _dbContext = dbcontext;

    }
}
