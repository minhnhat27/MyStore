using MyStore.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Admin.Request
{
    public class UpdateProductRequest
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public required string Gender { get; set; }
        public int Category { get; set; }
        public int Brand { get; set; }
        public required List<int> Materials { get; set; }
        public required IEnumerable<SizeAndQuantity> Sizes { get; set; }
    }
}
