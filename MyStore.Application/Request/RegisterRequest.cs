using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Request
{
    public class RegisterRequest
    {
        public required string Name { get; set; }
        public required string Password { get; set; }
        public required string Email { get; set; }
        public required string VerifyCode { get; set; }
    }
}
