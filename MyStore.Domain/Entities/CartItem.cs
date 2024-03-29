using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(UserId))]
    public class CartItem
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public required string UserId { get; set; }
        public User? User { get; set; }

        public int Quantity { get; set; }
    }
}
