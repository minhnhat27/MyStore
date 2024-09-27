using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class Repository<T>(MyDbContext dbcontext) : IRepository<T> where T : class
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbContext.Set<T>().ToArrayAsync();
        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression) 
            => await _dbContext.Set<T>().Where(expression).ToArrayAsync();
        public virtual async Task<T?> FindAsync(params object?[]? keyValues) 
            => await _dbContext.FindAsync<T>(keyValues);
        public virtual async Task AddAsync(T entity)
        {
            await _dbContext.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task AddAsync(IEnumerable<T> entities)
        {
            await _dbContext.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task DeleteAsync(params object?[]? keyValues)
        {
            var entity = await _dbContext.FindAsync<T>(keyValues);
            if (entity == null)
            {
                throw new ArgumentException($"Entity with specified keys not found.");
            }
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task DeleteAsync(T entity)
        {
            _dbContext.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            _dbContext.RemoveRange(entities);
            await _dbContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbContext.Update(entity);
            await _dbContext.SaveChangesAsync();
        }
        public virtual async Task UpdateAsync(IEnumerable<T> entities)
        {
            _dbContext.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
        }
        //public virtual async Task<IEnumerable<T>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<T, TKey>> orderBy)
        //    =>  await _dbContext.Set<T>()
        //        .OrderBy(orderBy)
        //        .Paginate(page, pageSize)
        //        .ToArrayAsync();
        public virtual async Task<IEnumerable<T>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderBy)
            => expression == null 
            ? await _dbContext.Set<T>().OrderBy(orderBy).Paginate(page, pageSize).ToArrayAsync()
            : await _dbContext.Set<T>().Where(expression).OrderBy(orderBy).Paginate(page, pageSize).ToArrayAsync();

        //public virtual async Task<IEnumerable<T>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<T, TKey>> orderByDesc) 
        //    => await _dbContext.Set<T>()
        //        .OrderBy(orderByDesc)
        //        .Paginate(page, pageSize)
        //        .ToArrayAsync();
        public virtual async Task<IEnumerable<T>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<T, bool>>? expression, Expression<Func<T, TKey>> orderByDesc) 
            => expression == null 
            ? await _dbContext.Set<T>().OrderByDescending(orderByDesc).Paginate(page, pageSize).ToArrayAsync() 
            : await _dbContext.Set<T>().Where(expression).OrderByDescending(orderByDesc).Paginate(page, pageSize).ToArrayAsync();
        public virtual async Task<int> CountAsync()
           => await _dbContext.Set<T>().CountAsync();
        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> expression)
           => await _dbContext.Set<T>()
                .Where(expression)
                .CountAsync();

        public virtual async Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> expression)
            => await _dbContext.Set<T>().SingleOrDefaultAsync(expression);

        public virtual async Task<T> SingleAsync(Expression<Func<T, bool>> expression)
            => await _dbContext.Set<T>().SingleAsync(expression);
    }
}
