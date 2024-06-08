using MyStore.Application.Model;
using MyStore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Admin.Response
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Enable { get; set; }
        public int Sold { get; set; }
        public string Gender { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string ImageUrl { get; set; }
    }
}
