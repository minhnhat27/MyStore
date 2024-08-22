using System.Linq.Expressions;

namespace MyStore.Application.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression);
        Task<T?> FindAsync(params object?[]? keyValues);
        Task AddAsync(T entity);
        Task AddAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(params object?[]? keyValues);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        //Task<IEnumerable<T>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<T, TKey>> orderBy);
        Task<IEnumerable<T>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderBy);
        //Task<IEnumerable<T>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<T, TKey>> orderByDesc);
        Task<IEnumerable<T>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderByDesc);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<T, bool>> expression);
    }
}
