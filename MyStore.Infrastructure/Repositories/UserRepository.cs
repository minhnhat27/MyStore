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

        public async Task<IEnumerable<User>> GetPagedAsync(int page, int pageSize, string key)
        {
            return await _dbContext.Users
                .Where(e => e.Id.Contains(key)
                    || (e.Fullname != null && e.Fullname.Contains(key))
                    || (e.Email != null && e.Email.Contains(key))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(key)))
                .Paginate(page, pageSize)
                .ToListAsync();
        }
        public async Task<int> CountAsync(string key)
        {
            return await _dbContext.Users
                .Where(e => e.Id.Contains(key)
                    || (e.Fullname != null && e.Fullname.Contains(key))
                    || (e.Email != null && e.Email.Contains(key))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(key)))
                .CountAsync();
        }
    }
}
