using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;

namespace MyStore.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MyDbContext _Dbcontext;
        public UserRepository(MyDbContext dbcontext) => _Dbcontext = dbcontext;

        public async Task<int> CountAsync()
        {
            return await _Dbcontext.Users.CountAsync();
        }

        public async Task<int> CountAsync(string key)
        {
            return await _Dbcontext.Users
                .Where(e => e.Id.Contains(key)
                    || (e.Fullname != null && e.Fullname.Contains(key))
                    || (e.Email != null && e.Email.Contains(key))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(key)))
                .CountAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize)
        {
            return await _Dbcontext.Users
                .Paginate(page, pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(int page, int pageSize, string key)
        {
            return await _Dbcontext.Users
                .Where(e => e.Id.Contains(key)
                    || (e.Fullname != null && e.Fullname.Contains(key))
                    || (e.Email != null && e.Email.Contains(key))
                    || (e.PhoneNumber != null && e.PhoneNumber.Contains(key)))
                .Paginate(page, pageSize)
                .ToListAsync();
        }
    }
}
