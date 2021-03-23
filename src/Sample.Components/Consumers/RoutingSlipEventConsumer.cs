using GreenPipes.Util;
using MassTransit;
using MassTransit.Courier.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.Consumers
{
    public class RoutingSlipEventConsumer : IConsumer<RoutingSlipCompleted>, 
        IConsumer<RoutingSlipActivityCompleted>
        , IConsumer<RoutingSlipFaulted>
    {
        readonly ILogger<RoutingSlipEventConsumer> _Logger;
        public RoutingSlipEventConsumer(ILogger<RoutingSlipEventConsumer> logger)
        {
            _Logger = logger;
        }
        public Task Consume(ConsumeContext<RoutingSlipCompleted> context)
        {
            if (_Logger.IsEnabled(LogLevel.Information))
            {
                _Logger.Log(LogLevel.Information, "Routing slip completed: {TrackingNumber}", context.Message.TrackingNumber);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipActivityCompleted> context)
        {
            if (_Logger.IsEnabled(LogLevel.Information))
            {
                _Logger.Log(LogLevel.Information,
                    "Routing slip activity completed: {TrackingNumber} {ActivityName}"
                    , context.Message.TrackingNumber, context.Message.ActivityName);
            }
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<RoutingSlipFaulted> context)
        {
            if (_Logger.IsEnabled(LogLevel.Information))
            {
                _Logger.Log(LogLevel.Information, "Routing slip falted: {TrackingNumber} {ExceptionInfo}", 
                    context.Message.TrackingNumber, context.Message.ActivityExceptions.FirstOrDefault());
            }
            return Task.CompletedTask;
        }
    }
}
