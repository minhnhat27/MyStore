using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Users
{
    public class UserRepository(MyDbContext dbcontext) : Repository<User>(dbcontext), IUserRepository
    {
        private readonly MyDbContext _dbcontext = dbcontext;

        public async Task<IEnumerable<User>> GetAsyncIncludeUserVoucher(Expression<Func<User, bool>> expression)
        {
            return await _dbcontext.Users
                .Where(expression)
                .Include(e => e.UserVouchers)
                .ToArrayAsync();
        }
    }
}
