﻿using MassTransit;
using MassTransit.Courier;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class FulfillOrderConsumer : IConsumer<FulfillOrder>
    {
        public async Task Consume(ConsumeContext<FulfillOrder> context)
        {
            var builder = new RoutingSlipBuilder(NewId.NextGuid());
            builder.AddActivity("AllocateInventory",
                new Uri("queue:allocate-inventory_execute"),
                new
                {
                    ItemNumber = "ITEM123",
                    Quantity = 10.0m
                });
            builder.AddVariable(nameof(context.Message.OrderId), context.Message.OrderId);
            var routingSlip = builder.Build();
            await context.Execute(routingSlip);
        }
    }
}
