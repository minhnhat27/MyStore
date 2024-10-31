namespace MyStore.Application.Response
{
    public class StatisticsResponse
    {
        public IEnumerable<StatisticData> Statistics { get; set; }
        public double Total { get; set; }
    }

    public class StatisticData
    {
        public int? Month { get; set; }
        public double Revenue { get; set; }
        public int TotalOrders { get; set; }
    }

    public class GeneralStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
    }

    //public class MonthRevenue
    //{
    //    public int Month { get; set; }
    //    public double Revenue { get; set; }
    //    public int TotalOrders { get; set; }
    //}
}
