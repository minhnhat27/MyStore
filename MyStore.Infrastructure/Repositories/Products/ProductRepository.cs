using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository.Products;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly MyDbContext _dbContext;
        public ProductRepository(MyDbContext dbcontext) : base(dbcontext) => _dbContext = dbcontext;

        public async Task<Product?> SingleOrDefaultAsync(int id)
        {
            return await _dbContext.Products
            .Include(e => e.Images)
            .Include(e => e.Materials)
            .Include(e => e.Sizes)
            .Include(e => e.Category)
            .Include(e => e.Brand)
            .SingleOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Product>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<Product, bool>>? expression, Expression<Func<Product, TKey>> orderBy)
            => expression == null
                ? await _dbContext.Products
                        .Include(e => e.Category)
                        .Include(e => e.Brand).OrderBy(orderBy).Paginate(page, pageSize).ToArrayAsync()
                : await _dbContext.Products
                        .Include(e => e.Category)
                        .Include(e => e.Brand).Where(expression).OrderBy(orderBy).Paginate(page, pageSize).ToArrayAsync();
        public override async Task<IEnumerable<Product>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<Product, bool>>? expression, Expression<Func<Product, TKey>> orderByDesc)
            => expression == null
                ? await _dbContext.Products
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .OrderByDescending(orderByDesc).Paginate(page, pageSize).ToArrayAsync()
                : await _dbContext.Products
                        .Include(e => e.Category)
                        .Include(e => e.Brand)
                        .Where(expression)
                        .OrderByDescending(orderByDesc).Paginate(page, pageSize).ToArrayAsync();
    }
}
