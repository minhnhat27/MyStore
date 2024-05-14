using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class Material
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<ProductMaterial> Products { get; } = new HashSet<ProductMaterial>();
    }
}
