using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Admin.Request
{
    public class CreateSizeRequest
    {
        public string Name { get; set; }
        public string? Description { get; set; }
    }
}
