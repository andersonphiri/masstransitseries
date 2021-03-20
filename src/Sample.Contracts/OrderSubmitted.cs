using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Contracts
{
    public interface OrderSubmitted
    {
        Guid OrderId { get; }
        string CustomerNumber { get; }
        DateTimeOffset Timestamp { get; }
    }
}
