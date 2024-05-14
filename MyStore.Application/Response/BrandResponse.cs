using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Response
{
    public class BrandResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Base64String { get; set; }
    }
}
