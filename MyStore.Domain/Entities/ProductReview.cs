using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public class ProductReview
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public required string Review { get; set; }
        public int Star { get; set; } = 0;

        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
