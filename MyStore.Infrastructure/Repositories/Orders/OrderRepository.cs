using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System.Linq.Expressions;

namespace MyStore.Infrastructure.Repositories.Orders
{
    public class OrderRepository(MyDbContext context) : Repository<Order>(context), IOrderRepository
    {
        private readonly MyDbContext _dbContext = context;

        public Task<Order?> SingleOrDefaultAsyncInclude(Expression<Func<Order, bool>> expression)
        {
            return _dbContext.Orders
                .Include(e => e.OrderDetails)
                .SingleOrDefaultAsync(expression);
        }

        public async Task<IEnumerable<Order>> GetPagedOrderByDescendingAsyncInclude<TKey>(int page, int pageSize, Expression<Func<Order, bool>>? expression, Expression<Func<Order, TKey>> orderByDesc)
        => expression == null
            ? await _dbContext.Orders
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.OrderDetails)
                .ToArrayAsync()
            : await _dbContext.Orders
                .Where(expression)
                .OrderByDescending(orderByDesc)
                .Paginate(page, pageSize)
                .Include(e => e.OrderDetails).ToArrayAsync();
        
        public async Task<double> GetRevenue(Expression<Func<Order, bool>>? expression)
            => expression != null ?
                await _dbContext.Orders
                    .Where(expression)
                    .Select(x => x.Total).SumAsync()
                : await _dbContext.Orders
                    .Select(x => x.Total).SumAsync();

        //public async Task<IEnumerable<MonthRevenue>> GetRevenue12Month(int year)
        //    => await _dbContext.Orders
        //            .Where(e => e.ReceivedDate.HasValue && e.ReceivedDate.Value.Year.Equals(year))
        //            .GroupBy(g => new { g.ReceivedDate!.Value.Year, g.ReceivedDate.Value.Month })
        //            .Select(x => new MonthRevenue
        //            {
        //                Month = x.Key.Month,
        //                Revenue = x.Sum(x => x.Total),
        //                TotalOrders = x.Count()
        //            }).ToArrayAsync();

        public async Task<IEnumerable<StatisticData>> GetRevenue12Month(Expression<Func<Order, bool>> expression)
            => await _dbContext.Orders
                    .Where(expression)
                    .GroupBy(g => new { g.OrderDate.Year, g.OrderDate.Month })
                    .Select(x => new StatisticData
                    {
                        Month = x.Key.Month,
                        Revenue = x.Sum(x => x.Total),
                        TotalOrders = x.Count()
                    }).ToArrayAsync();
    }
}
