using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductRepository(MyDbContext dbcontext) : Repository<Product>(dbcontext), IProductRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;

        public async Task<Product?> SingleOrDefaultAsyncInclude(Expression<Func<Product, bool>> expression)
        {
            return await _dbContext.Products
                        .Where(expression)
                        .Include(e => e.Images)
                        .Include(e => e.Materials)
                        .Include(e => e.ProductColors)
                            .ThenInclude(e => e.ProductSizes)
                                .ThenInclude(e => e.Size)
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .AsSplitQuery()
                        .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<string>> GetName(Expression<Func<Product, bool>> expression)
        => await _dbContext.Products.Where(expression).OrderBy(e => e.Name).Take(10).Select(x => x.Name).ToArrayAsync();

        public override async Task<IEnumerable<Product>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<Product, bool>>? expression, Expression<Func<Product, TKey>> orderBy)
            => expression == null
                ? await _dbContext.Products
                        .Paginate(page, pageSize)
                        .Include(e => e.Images)
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .Include(e => e.ProductReviews)
                        .AsSingleQuery()
                        .OrderBy(orderBy).ToArrayAsync()
                : await _dbContext.Products
                        .Where(expression)
                        .Paginate(page, pageSize)
                        .Include(e => e.Images)
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .Include(e => e.ProductReviews)
                        .AsSingleQuery()
                        .OrderBy(orderBy).ToArrayAsync();
        public override async Task<IEnumerable<Product>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<Product, bool>>? expression, Expression<Func<Product, TKey>> orderByDesc)
            => expression == null
                ? await _dbContext.Products
                        .OrderByDescending(orderByDesc)
                        .Paginate(page, pageSize)
                        .Include(e => e.Images)
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .Include(e => e.ProductReviews)
                        .AsSingleQuery()
                        .ToArrayAsync()
                : await _dbContext.Products
                        .Where(expression)
                        .OrderByDescending(orderByDesc)
                        .Paginate(page, pageSize)
                        .Include(e => e.Images)
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .Include(e => e.ProductReviews)
                        .Include(e => e.ProductFavorites)
                        .AsSingleQuery()
                        .ToArrayAsync();
    }
}
