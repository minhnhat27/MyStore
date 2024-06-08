using Microsoft.EntityFrameworkCore;
using MyStore.Application.IRepository;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.IQueryableExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationContext _DbContext;
        public OrderRepository(ApplicationContext context) => _DbContext = context;
        public async Task AddOrderAsync(Order order)
        {
            await _DbContext.AddAsync(order);
            await _DbContext.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(Order order)
        {
            _DbContext.Remove(order);
            await _DbContext.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _DbContext.Orders.FindAsync(id);
        }

        public async Task<IList<Order>> GetOrdersAsync()
        {
            return await _DbContext.Orders.ToListAsync();
        }

        public async Task UpdateOrderAsync(Order order)
        {
            _DbContext.Update(order);
            await _DbContext.SaveChangesAsync();
        }

        public async Task<IList<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _DbContext.Orders.Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task<IList<Order>> GetOrdersAsync(int page, int size)
        {
            return await _DbContext.Orders.Paginate(page, size).ToListAsync();
        }

        public async Task<int> CountAsync()
        {
            return await _DbContext.Orders.CountAsync();
        }

        public async Task<IList<Order>> GetOrdersAsync(int page, int size, string keySearch)
        {
            return await _DbContext.Orders
                .Where(e => e.Id.ToString().Equals(keySearch) 
                    || e.OrderStatusName.Contains(keySearch)
                    || e.PaymentMethodName.Equals(keySearch))
                .Paginate(page, size).ToListAsync();
        }

        public async Task<int> CountAsync(string keySearch)
        {
            return await _DbContext.Orders
                .Where(e => e.Id.ToString().Equals(keySearch)
                    || e.OrderStatusName.Contains(keySearch)
                    || e.PaymentMethodName.Equals(keySearch)).CountAsync();
        }

        public Task AddOrderStatusAsync(OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public Task DeleteOrderStatusAsync(OrderStatus status)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<PaymentMethod>> GetPaymentMethodsAsync()
        {
            return await _DbContext.PaymentMethods.ToListAsync();
        }
    }
}
