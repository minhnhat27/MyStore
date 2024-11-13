using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class BrandRepository(MyDbContext dbcontext) : Repository<Brand>(dbcontext), IBrandRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public override async Task<IEnumerable<Brand>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<Brand, bool>>? expression, Expression<Func<Brand, TKey>> orderByDesc)
            => expression == null
                ? await _dbContext.Brands
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.Products)
                    .ToArrayAsync()
                : await _dbContext.Brands
                    .Where(expression)
                    .OrderByDescending(orderByDesc)
                    .Paginate(page, pageSize)
                    .Include(e => e.Products)
                    .ToArrayAsync();
    }
}
