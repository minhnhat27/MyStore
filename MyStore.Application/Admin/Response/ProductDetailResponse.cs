using MyStore.Application.Model;
using MyStore.Application.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Admin.Response
{
    public class ProductDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Enable { get; set; }
        public int Sold { get; set; }
        public string Gender { get; set; }
        public int Category { get; set; }
        public int Brand { get; set; }
        public List<int> Materials { get; set; }
        public List<int> Sizes { get; set; }
        public List<SizeAndQuantity> SizeQuantity { get; set; }
        public List<string> Images { get; set; }
    }
}
