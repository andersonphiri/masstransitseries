using MassTransit;
using Microsoft.Extensions.Logging;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumer : IConsumer<ISubmitOrder>
    {
        ILogger<SubmitOrderConsumer> _Logger;
        public SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger)
        {
            _Logger = logger;
        }
        public async Task Consume(ConsumeContext<ISubmitOrder> context)
        {
            _Logger.LogInformation("SubmitOrderConsumer: {CustomerNumber}", context.Message.CustomerNumber);
            if (context.Message.CustomerNumber.Contains("TESt", StringComparison.InvariantCultureIgnoreCase))
            {
                if (context.RequestId != null)
                {
                    await context.RespondAsync<IOrderSubmissionRejected>(new
                    {

                        OrderId = context.Message.OrderId,
                        InVar.Timestamp,
                        CustomerNumber = context.Message.CustomerNumber,
                        Reasons = new[] { $"Test Customer cannot submit orders: {context.Message.CustomerNumber}" }
                    });
                }
                
                return;
            }
            if (context.RequestId != null)
            {
                await context.RespondAsync<IOrderSubmissionAccepted>(new
                {
                    InVar.Timestamp
,
                    OrderId = context.Message.OrderId,
                    CustomerNumber = context.Message.CustomerNumber
                });
            }
            
        }
    }
}
