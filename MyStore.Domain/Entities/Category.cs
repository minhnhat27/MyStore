using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<Product> Products { get; } = new HashSet<Product>();
    }
}
