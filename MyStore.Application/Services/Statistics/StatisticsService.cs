using MyStore.Application.IRepositories.Orders;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.IRepositories.Users;
using MyStore.Application.Response;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

namespace MyStore.Application.Services.Statistics
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;

        public StatisticsService(IOrderRepository orderRepository, IUserRepository userRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userRepository = userRepository;
        }

        public async Task<GeneralStatistics> GeneralStatistics()
        {
            var orders = await _orderRepository.CountAsync();
            var products = await _productRepository.CountAsync();
            var users = await _userRepository.CountAsync();

            return new GeneralStatistics
            {
                TotalOrders = orders,
                TotalProducts = products,
                TotalUsers = users,
            };
        }


        public async Task<StatisticsResponse> GetRevenue()
        {
            Expression<Func<Order, bool>>
               expression = e => e.OrderStatus == DeliveryStatusEnum.Received;

            var revenue = await _orderRepository
                .GetRevenue(expression);
            var total = await _orderRepository.CountAsync(expression);
            return new StatisticsResponse
            {
                Revenue = revenue,
                TotalOrders = total
            };
        }

        public async Task<StatisticsResponse> GetRevenue(DateTime start, DateTime end)
        {
            Expression<Func<Order, bool>>
                expression = e => e.ReceivedDate >= start && e.ReceivedDate <= end 
                && e.OrderStatus == DeliveryStatusEnum.Received;

            var revenue = await _orderRepository
                .GetRevenue(expression);
            var total = await _orderRepository.CountAsync(expression);
            return new StatisticsResponse
            {
                Revenue = revenue,
                TotalOrders = total
            };
        }

        public async Task<StatisticsResponse> GetRevenue(int month, int year)
        {
            Expression<Func<Order, bool>>
                expression = e => e.OrderStatus == DeliveryStatusEnum.Received &&
                e.ReceivedDate.HasValue && e.ReceivedDate.Value.Month.Equals(month) && 
                e.ReceivedDate.Value.Year.Equals(year); 

            var revenue = await _orderRepository
                .GetRevenue(expression);
            var total = await _orderRepository.CountAsync(expression);
            return new StatisticsResponse
            {
                Revenue = revenue,
                TotalOrders = total
            };
        }

        public async Task<StatisticsResponse> GetRevenue(int year)
        {
            Expression<Func<Order, bool>>
                expression = e => e.OrderStatus == DeliveryStatusEnum.Received &&
                e.ReceivedDate.HasValue &&
                e.ReceivedDate.Value.Year.Equals(year);

            var revenue = await _orderRepository
                .GetRevenue(expression);
            var total = await _orderRepository.CountAsync(expression);
            return new StatisticsResponse
            {
                Revenue = revenue,
                TotalOrders = total
            };
        }

        public async Task<IEnumerable<MonthRevenue>> GetRevenueInYear(int? year)
        {
            var currentYear = year ?? DateTime.Now.Year;
            var revenueList = new List<MonthRevenue>();

            var revenues = await _orderRepository.GetRevenue12Month(currentYear);

            for (int month = 1; month <= 12; month++)
            {
                var monthRevenue = revenues.FirstOrDefault(r => r.Month == month) 
                    ?? new MonthRevenue
                    {
                        TotalOrders = 0,
                        Month = month,
                        Revenue = 0
                    };
                revenueList.Add(monthRevenue);
            }

            return revenueList;
        }

        public Task<int> OrderNumber()
        {
            throw new NotImplementedException();
        }

        public Task<int> OrderNumber(int month, int year)
        {
            throw new NotImplementedException();
        }
    }
}
