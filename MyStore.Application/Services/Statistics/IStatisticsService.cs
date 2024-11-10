using MyStore.Application.Response;

namespace MyStore.Application.Services.Statistics
{
    public interface IStatisticsService
    {
        Task<GeneralStatistics> GeneralStatistics();

        Task<StatisticData> GetRevenue();
        Task<StatisticData> GetRevenue(DateTime start, DateTime end);
        Task<StatisticData> GetRevenue(int month, int year);
        Task<StatisticData> GetRevenue(int year);
        Task<StatisticsResponse> GetRevenueInYear(int? year = null);

        Task<int> OrderNumber();
        Task<int> OrderNumber(int month, int year);
    }
}
