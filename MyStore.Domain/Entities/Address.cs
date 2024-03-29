using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Address
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        
        public required string UserId { get; set; }
        public User? User { get; set; }
    }
}
