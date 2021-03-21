using Automatonymous;
using GreenPipes;
using MassTransit;
using MassTransit.Definition;
using MassTransit.RedisIntegration;
using MassTransit.Saga;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.StateMachines
{
    public class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
        public OrderStateMachine()
        {
            // correlation
            Event(() => OrderStatusRequested, 
                x => { 
                    x.CorrelateById(m => m.Message.OrderId);
                    //handle if order not found from specified ID
                    x.OnMissingInstance(m => m.ExecuteAsync(async context =>
                    {
                        if (context.RequestId.HasValue)
                        {
                            await context.RespondAsync<OrderNotFound>(new { context.Message.OrderId });
                        }
                    }));
                });
            Event(() => OrderSubmitted, 
                x => 
                {
                    x.CorrelateById(m => m.Message.OrderId);
                });
            InstanceState(x => x.CurrentState);
            Initially(
                When(OrderSubmitted)
                 .Then(context =>
                 {
                     context.Instance.SubmitDate = context.Data.Timestamp;
                     context.Instance.CustomerNumber = context.Data.CustomerNumber;
                     context.Instance.Updated = DateTimeOffset.Now;
                 }
                 )
                .TransitionTo(Submitted)
               
                );
            //if i have already tried to submit, just ignore and do not publish a fault
            During(Submitted, Ignore(OrderSubmitted));
            // handle check order Status request
            DuringAny(
                When(OrderStatusRequested)
                    .RespondAsync(x => x.Init<OrderStatus>(new
                    {
                        OrderId = x.Instance.CorrelationId,
                        State = x.Instance.CurrentState
                    }))
                );
            // messages are never guaranteed to be in order and you cant move from Submitted State back to Created
            DuringAny(When(OrderSubmitted)
                .Then(
                    context =>
                    {
                        context.Instance.SubmitDate ??= context.Data.Timestamp;
                        context.Instance.CustomerNumber ??= context.Data.CustomerNumber;
                    }
                )
                );

        }

        public State Submitted { get; private set; }
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        // handle check order status request
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
    }

    public class OrderState : SagaStateMachineInstance, ISagaVersion
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string CustomerNumber { get; set; }
        public int Version { get ; set; }
        public DateTimeOffset? SubmitDate { get; set; }
    }

    public class OrderStateMachineDefinition : SagaDefinition<OrderState>
    {
        public OrderStateMachineDefinition()
        {
            ConcurrentMessageLimit = 4;
        }
        protected override void ConfigureSaga(IReceiveEndpointConfigurator endpointConfigurator, ISagaConfigurator<OrderState> sagaConfigurator)
        {
            endpointConfigurator.UseMessageRetry(r => r.Intervals(500, 5000, 10000));
            endpointConfigurator.UseInMemoryOutbox();
        }
    }

}
