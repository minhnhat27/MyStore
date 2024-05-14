using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(ProductId), nameof(SizeId))]
    public class ProductSize
    {
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int SizeId { get; set; }
        public Size Size { get; set; }

        [Range(1000, double.MaxValue)]
        public double Price { get; set; }
        [Range(0, int.MaxValue)]
        public int InStock { get; set; }
        [Range(0, 100)]
        public double DiscountPercent { get; set; }
    }
}
