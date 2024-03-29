using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Domain.Enumerations
{
    internal enum DeliveryStatus
    {
        Proccessing,
        Confirmed,
        Shipping,
        Received,
        Canceled,
    }
}
