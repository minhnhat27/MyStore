using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.IRepository
{
    public interface IOrderRepository
    {
        Task<IList<Order>> GetOrdersAsync();
        Task<IList<Order>> GetOrdersAsync(int page, int size);
        Task<IList<Order>> GetOrdersAsync(int page, int size, string keySearch);
        Task<int> CountAsync();
        Task<int> CountAsync(string keySearch);
        Task<IList<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order?> GetOrderByIdAsync(int id);
        Task AddOrderAsync(Order order);
        Task UpdateOrderAsync(Order order);
        Task DeleteOrderAsync(Order order);
    }
}
