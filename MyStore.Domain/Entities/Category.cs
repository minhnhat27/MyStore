using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Category : IBaseEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get; } = new HashSet<Product>();
    }
}
