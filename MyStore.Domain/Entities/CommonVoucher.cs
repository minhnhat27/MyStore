﻿using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class CommonVoucher
    {
        [Key]
        public string Code { get; set; }

        public bool IsActive { get; set; }

        //public int QuantityUsed { get; set; }
        //public int MaximumUse { get; set; }

        public int? DiscountPercent { get; set; }
        public double? DiscountAmount { get; set; }

        public double MinOrder { get; set; }
        public double? MaxDiscount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
