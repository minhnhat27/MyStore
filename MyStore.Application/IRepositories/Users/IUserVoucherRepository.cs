using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Users
{
    public interface IUserVoucherRepository : IRepository<UserVoucher>
    {
        Task<UserVoucher?> SingleOrDefaultAsyncInclude(Expression<Func<UserVoucher, bool>> expression);
    }
}
