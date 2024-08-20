using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class Repository<T>(MyDbContext dbcontext) : IRepository<T> where T : class
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public virtual async Task<IEnumerable<T>> GetAllAsync() => await _dbContext.Set<T>().ToListAsync();
        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> expression) 
            => await _dbContext.Set<T>().Where(expression).ToListAsync();
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

        public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize)
            => await _dbContext.Set<T>().Paginate(page, pageSize).ToListAsync();
        public virtual async Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, 
            Expression<Func<T, bool>> filters, Expression<Func<T, bool>> sorter)
            => await _dbContext.Set<T>().Where(filters).OrderBy(sorter).Paginate(page, pageSize).ToListAsync();
        public async Task<int> CountAsync()
           => await _dbContext.Set<T>().CountAsync();
        public async Task<int> CountAsync(Expression<Func<T, bool>> filters, Expression<Func<T, bool>> sorter)
            => await _dbContext.Set<T>().Where(filters).CountAsync();
    }
}
