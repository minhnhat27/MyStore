using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Users
{
    public class UserRepository(MyDbContext dbcontext) : Repository<User>(dbcontext), IUserRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public async Task<IEnumerable<User>> GetAsyncIncludeUserVoucher(Expression<Func<User, bool>> expression)
        {
            return await _dbContext.Users
                .Where(expression)
                .Include(e => e.UserVouchers)
                .ToArrayAsync();
        }

        public override async Task<IEnumerable<User>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<User, bool>>? expression, Expression<Func<User, TKey>> orderByDesc)
         => expression == null
            ? await _dbContext.Users
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(x => x.UserRoles)
                    .ThenInclude(e => e.Role)
                .ToArrayAsync()
            : await _dbContext.Users
                .Where(expression)
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(x => x.UserRoles)
                    .ThenInclude(e => e.Role)
                .ToArrayAsync();
    }
}
