using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Drawing.Printing;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductFavoriteRepository(MyDbContext dbcontext) : Repository<ProductFavorite>(dbcontext), IProductFavoriteRepository
    {
        private readonly MyDbContext _dbcontext = dbcontext;

        public override async Task<IEnumerable<ProductFavorite>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<ProductFavorite, bool>>? expression, Expression<Func<ProductFavorite, TKey>> orderBy)
        => expression == null 
            ? await _dbcontext.ProductFavorites
                .OrderBy(orderBy)
                .Paginate(page, pageSize)
                .Include(e => e.Product)
                    .ThenInclude(e => e.Images)
                .ToArrayAsync()
            : await _dbcontext.ProductFavorites
                .Where(expression)
                .OrderBy(orderBy)
                .Paginate(page, pageSize)
                .Include(e => e.Product)
                    .ThenInclude(e => e.Images)
                .ToArrayAsync();
    }
}
