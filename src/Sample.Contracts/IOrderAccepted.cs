using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Contracts
{
    public interface IOrderAccepted
    {
        Guid OrderId { get; }
        DateTimeOffset Timestamp { get; }
    }
}
