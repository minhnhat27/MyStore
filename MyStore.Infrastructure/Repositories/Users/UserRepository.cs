using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;

namespace MyStore.Infrastructure.Repositories.Users
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(MyDbContext dbcontext) : base(dbcontext) { }

    }
}
