using MassTransit;
using MassTransit.ConsumeConfigurators;
using MassTransit.Definition;
using System;
using GreenPipes;

namespace Sample.Components.Consumers
{
    public class SubmitOrderConsumerDefinition : ConsumerDefinition<SubmitOrderConsumer>
    {
        protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator, IConsumerConfigurator<SubmitOrderConsumer> consumerConfigurator)
        {
           // base.ConfigureConsumer(endpointConfigurator, consumerConfigurator);
            endpointConfigurator.UseMessageRetry(retry => retry.Interval(3, 1000));
            endpointConfigurator.UseExecute((context) => Console.WriteLine($"Consuming From Address: {context.SourceAddress}"));
            //consumerConfigurator.Message<SubmitOrderConsumer>(configure =>
            //{
            //   // configure.
            //});
        }
    }

}
