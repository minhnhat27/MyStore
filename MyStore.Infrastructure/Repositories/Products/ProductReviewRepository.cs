using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Products;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Products
{
    public class ProductReviewRepository(MyDbContext dbcontext) : Repository<ProductReview>(dbcontext), IProductReviewRepository
    {
        private readonly MyDbContext _dbContext = dbcontext;
        public override async Task<IEnumerable<ProductReview>> GetPagedAsync<TKey>(int page, int pageSize, Expression<Func<ProductReview, bool>>? expression, Expression<Func<ProductReview, TKey>> orderBy)
            => expression == null
                ? await _dbContext.ProductReviews
                    .OrderBy(orderBy)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync()
                : await _dbContext.ProductReviews
                    .Where(expression)
                    .OrderBy(orderBy)
                    .Paginate(page, pageSize)
                    .Include(e => e.User).ToArrayAsync();
        public override async Task<IEnumerable<ProductReview>> GetPagedOrderByDescendingAsync<TKey>(int page, int pageSize, Expression<Func<ProductReview, bool>>? expression, Expression<Func<ProductReview, TKey>> orderByDesc)
             => expression == null
                    ? await _dbContext.ProductReviews
                        .OrderByDescending(orderByDesc)
                        .Paginate(page, pageSize)
                        .Include(e => e.User).ToArrayAsync()
                    : await _dbContext.ProductReviews
                        .Where(expression)
                        .OrderByDescending(orderByDesc)
                        .Paginate(page, pageSize)
                        .Include(e => e.User).ToArrayAsync();
    }
}
