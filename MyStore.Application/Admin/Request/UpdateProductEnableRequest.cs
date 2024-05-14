using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Admin.Request
{
    public class UpdateProductEnableRequest
    {
        public int Id { get; set; }
        public bool Enable { get; set; }
    }
}
