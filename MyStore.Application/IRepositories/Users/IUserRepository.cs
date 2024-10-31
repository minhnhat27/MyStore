using MyStore.Domain.Entities;
using System.Linq.Expressions;

namespace MyStore.Application.IRepositories.Users
{
    public interface IUserRepository : IRepository<User>
    {
        Task<IEnumerable<User>> GetAsyncIncludeUserVoucher(Expression<Func<User, bool>> expression);
    }
}
