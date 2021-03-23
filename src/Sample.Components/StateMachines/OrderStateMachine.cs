
using MassTransit;
//using MassTransit.RedisIntegration;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Automatonymous;
using MassTransit.MongoDbIntegration;
using Sample.Components.StateMachines.OrderStateMachineActivities;

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

            Event(() => AccountClosed, x => x.CorrelateBy(
                (instance, context) => instance.CustomerNumber == context.Message.CustomerNumber));
            Event(() => OrderAccepted, x => x.CorrelateById(m => m.Message.OrderId));

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
            During(Submitted, 
                Ignore(OrderSubmitted)
                ,When(AccountClosed).TransitionTo(Cancelled)
                , When(OrderAccepted).Activity(x => x.OfType<AcceptOrderActivity>()).TransitionTo(Accepted)
                );
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
        public State Cancelled { get; private set; }
        public State Accepted { get; private set; }
        public Event<OrderSubmitted> OrderSubmitted { get; private set; }
        public Event<OrderAccepted> OrderAccepted { get; private set; }
        public Event<CheckOrder> OrderStatusRequested { get; private set; }
        public Event<ICustomerAccountClosed> AccountClosed { get; private set; }
        
    }


}
