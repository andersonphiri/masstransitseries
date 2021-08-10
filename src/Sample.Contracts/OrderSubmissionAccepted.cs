using System;

namespace Sample.Contracts
{
    public interface OrderSubmissionAccepted
    {
        Guid OrderId { get; }
        DateTimeOffset Timestamp { get; }
        string CustomerNumber { get; }
    }
}
