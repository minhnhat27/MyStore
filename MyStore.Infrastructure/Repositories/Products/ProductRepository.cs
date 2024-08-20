using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly MyDbContext _dbContext;
        public ProductRepository(MyDbContext dbcontext) : base(dbcontext) => _dbContext = dbcontext;

        public async Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize, string key)
        {
            return await _dbContext.Products
                    .Where(e => e.Name.Contains(key) || e.Id.ToString().Contains(key))
                    .Include(e => e.Category)
                    .Include(e => e.Brand)
                    .OrderByDescending(e => e.CreatedAt)
                    .Paginate(page, pageSize)
                    .ToListAsync();
        }

        public async Task<int> CountAsync(string key)
        {
            return await _dbContext.Products
                .Where(e => e.Name.Contains(key) || e.Id.ToString().Equals(key))
                .CountAsync();
        }

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

        public override async Task<IEnumerable<Product>> GetPagedAsync(int page, int pageSize)
        {
            return await _dbContext.Products
                    .Include(e => e.Category)
                    .Include(e => e.Brand)
                    .OrderByDescending(e => e.CreatedAt)
                    .Paginate(page, pageSize)
                    .ToListAsync();
        }
    }
}
