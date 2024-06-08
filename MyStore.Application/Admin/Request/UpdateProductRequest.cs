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
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public string Gender { get; set; }
        public int Category { get; set; }
        public int Brand { get; set; }
        public IList<int> Materials { get; set; }
        public IList<SizeAndQuantity> Sizes { get; set; }
        public IList<string>? ImagesUrl { get; set; }
    }
}
