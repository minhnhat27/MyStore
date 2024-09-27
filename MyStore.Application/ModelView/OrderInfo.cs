namespace MyStore.Application.ModelView
{
    public class OrderInfo
    {
        public long OrderId { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; }
        public string OrderDesc { get; set; }
        public DateTime CreatedDate { get; set; }

        //public string BillEmail { get; set; }
    }
}
