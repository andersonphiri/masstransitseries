using MassTransit;
using System;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Warehouse.Components
{
    public class AllocateInventoryConsumer : IConsumer<AllocateInventory>
    {
        public Task Consume(ConsumeContext<AllocateInventory> context)
        {
            throw new NotImplementedException();
        }
    }
}
