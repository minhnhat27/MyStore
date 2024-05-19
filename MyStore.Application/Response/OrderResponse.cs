using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Response
{
    public class OrderResponse
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
