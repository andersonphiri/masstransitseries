using Automatonymous;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sample.Components.StateMachines
{
    class OrderStateMachine : MassTransitStateMachine<OrderState>
    {
    }

    public class OrderState
    {
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
    }
}
