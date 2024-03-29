using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class PaymentMethod
    {
        [Key]
        public required string Name { get; set; }
        public ICollection<Order> Orders { get; } = new HashSet<Order>();
    }
}
