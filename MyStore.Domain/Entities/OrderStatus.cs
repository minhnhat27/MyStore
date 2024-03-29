using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class OrderStatus
    {
        [Key]
        public required string Name { get; set; }
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
