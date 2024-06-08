using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Entities
{
    public interface IBaseEntity
    {
        DateTime? CreatedAt { get; set; }
        DateTime? UpdatedAt { get; set; }
    }
}
