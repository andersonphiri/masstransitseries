using System;

namespace Sample.Contracts
{
    public interface ISubmitOrder
    {
        Guid OrderId { get; }
        DateTimeOffset Timestamp { get; }
        string CustomerNumber { get; }
    }
}
