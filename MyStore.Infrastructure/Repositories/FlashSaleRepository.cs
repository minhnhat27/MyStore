using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories
{
    public class FlashSaleRepository(MyDbContext dbcontext) : Repository<FlashSale>(dbcontext), IFlashSaleRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public override async Task<IEnumerable<FlashSale>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<FlashSale, bool>>? expression, Expression<Func<FlashSale, TKey>> orderBy)
        => expression == null
            ? await _dbContext.FlashSales
                .OrderBy(orderBy)
                .Paginate(page, pageSize)
                .Include(e => e.ProductFlashSales)
                .ToArrayAsync()
            : await _dbContext.FlashSales
                .Where(expression)
                .OrderBy(orderBy)
                .Paginate(page, pageSize)
                .Include(e => e.ProductFlashSales)
                .ToArrayAsync();

        public override async Task<IEnumerable<FlashSale>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<FlashSale, bool>>? expression, Expression<Func<FlashSale, TKey>> orderByDesc)
        => expression == null
            ? await _dbContext.FlashSales
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.ProductFlashSales)
                .ToArrayAsync()
            : await _dbContext.FlashSales
        .Where(expression)
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.ProductFlashSales)
                .ToArrayAsync();
    }
}
