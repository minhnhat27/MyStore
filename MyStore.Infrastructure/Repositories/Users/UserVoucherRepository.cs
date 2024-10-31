using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Users
{
    public class UserVoucherRepository(MyDbContext dbcontext) : Repository<UserVoucher>(dbcontext), IUserVoucherRepository
    {
        private readonly MyDbContext _dbcontext = dbcontext;
        public override async Task<IEnumerable<UserVoucher>> GetAsync(Expression<Func<UserVoucher, bool>> expression)
        {
            return await _dbcontext.UserVouchers
                .Where(expression)
                .Include(e => e.Voucher).ToArrayAsync();
        }

        public async Task<UserVoucher?> SingleOrDefaultAsyncInclude(Expression<Func<UserVoucher, bool>> expression)
        {
            return await _dbcontext.UserVouchers
                .Include(e => e.Voucher)
                .SingleOrDefaultAsync(expression);
        }
    }
}
