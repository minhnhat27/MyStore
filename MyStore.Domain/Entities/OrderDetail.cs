﻿using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace MyStore.Domain.Entities
{
    public class OrderDetail
    {
        public long Id { get; set; }

        public long OrderId { get; set; }
        public Order Order { get; set; }

        public long? ProductId { get; set; }
        [DeleteBehavior(DeleteBehavior.SetNull)]
        public Product Product { get; set; }

        [MaxLength(155)]
        public string ProductName { get; set; }

        //public string SizeName { get; set; }
        //public string ColorName { get; set; }
        public long SizeId { get; set; }
        public long ColorId { get; set; }
        [MaxLength(50)]
        public string Variant { get; set; }

        public double OriginPrice { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }
}
