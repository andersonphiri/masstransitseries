using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Contracts
{
    public interface OrderSubmissionRejected
    {
        Guid OrderId { get; }
        DateTimeOffset Timestamp { get; }
        string CustomerNumber { get; }
        List<string> Reasons { get; }
    }
}
