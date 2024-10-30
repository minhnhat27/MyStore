using MyStore.Application.Response;

namespace MyStore.Application.Services
{
    public interface IStatisticsService
    {
        Task<GeneralStatistics> GeneralStatistics();

        Task<StatisticsResponse> GetRevenue();
        Task<StatisticsResponse> GetRevenue(DateTime start, DateTime end);
        Task<StatisticsResponse> GetRevenue(int month, int year);
        Task<StatisticsResponse> GetRevenue(int year);

        Task<IEnumerable<MonthRevenue>> GetRevenueInYear(int? year = null);

        Task<int> OrderNumber();
        Task<int> OrderNumber(int month, int year);
    }
}
