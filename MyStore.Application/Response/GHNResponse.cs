namespace MyStore.Application.Response
{
    public class GHNData
    {
        public string Order_code { get; set; }
        //public string Sort_code { get; set; }
        //public double Total_fee { get; set; }
        public DateTime Expected_delivery_time { get; set; }
    }

    public class GHNResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public GHNData? Data { get; set; }
        public string Code_message { get; set; }
    }
}
