namespace MyStore.Application.DTO
{
    public class OrderDTO
    {
        public int Id { get; set; }
        public double Total { get; set; }
        public DateTime OrderDate { get; set; }
        public bool Paid { get; set; } = false;
        public required string PaymentMethod { get; set; }
        public required string OrderStatus { get; set; }
        public required string UserId { get; set; }
    }
}
