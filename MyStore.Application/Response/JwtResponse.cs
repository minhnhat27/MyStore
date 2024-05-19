using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Response
{
    public class JwtResponse
    {
        public string? Jwt { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
    }
}
