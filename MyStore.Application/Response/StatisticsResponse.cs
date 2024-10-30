namespace MyStore.Application.Response
{
    public class StatisticsResponse
    {
        public double Revenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class GeneralStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
    }

    public class MonthRevenue
    {
        public int Month { get; set; }
        public double Revenue { get; set; }
        public int TotalOrders { get; set; }
    }
}
