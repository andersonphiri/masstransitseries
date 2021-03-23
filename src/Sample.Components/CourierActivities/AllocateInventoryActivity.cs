using MassTransit;
using MassTransit.Courier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse.Contracts;

namespace Sample.Components.CourierActivities
{
    public class AllocateInventoryActivity : IActivity<AllocateInventoryArguments, AllocateInventoryLog>
    {
        private readonly IRequestClient<AllocateInventory> _client;
        public AllocateInventoryActivity(IRequestClient<AllocateInventory> client)
        {
            _client = client;
        }
        public async Task<CompensationResult> Compensate(CompensateContext<AllocateInventoryLog> context)
        {
            await context.Publish<AllocationReleaseRequested>(new
            {
                AllocationId = context.Log.AllocationId,
                Reason = "Reason for aloocation issue"
            });
            return context.Compensated();
        }

        public async Task<ExecutionResult> Execute(ExecuteContext<AllocateInventoryArguments> context)
        {
            var itemNumber = context.Arguments.ItemNumber;
            var orderId = context.Arguments.OrderId;
            var quantity = context.Arguments.Quantity;
            if (string.IsNullOrEmpty(itemNumber))
            {
                throw new ArgumentNullException(nameof(itemNumber));
            }
            if (quantity <= 0.0m)
            {
                throw new ArgumentNullException(nameof(quantity));
            }
            var allocationId = NewId.NextGuid();
            var response = await _client.GetResponse<InventoryAllocated>(new
            {
                AllocationId = allocationId,
                ItemNumber = itemNumber,
                Quantity = quantity
            });

            return context.Completed(new { AllocationId = allocationId });

        }
    }

    public interface AllocateInventoryArguments
    {
        Guid OrderId { get; }
        string ItemNumber { get; }
        decimal Quantity { get; }
    }
    public interface AllocateInventoryLog
    {
        Guid AllocationId { get; }
    }
}
