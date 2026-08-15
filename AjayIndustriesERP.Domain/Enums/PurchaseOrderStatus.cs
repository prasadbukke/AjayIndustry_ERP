using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AjayIndustriesERP.Domain.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft = 1,
        Confirmed = 2,
        Sent = 3,
        PartiallyReceived = 4,
        Received = 5,
        Closed = 6,
        Cancelled = 7
    }
}
