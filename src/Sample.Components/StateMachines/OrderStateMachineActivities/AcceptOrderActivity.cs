using Automatonymous;
using GreenPipes;
using Sample.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Components.StateMachines.OrderStateMachineActivities
{
    public class AcceptOrderActivity : Activity<OrderState, IOrderAccepted>
    {
        public void Accept(StateMachineVisitor visitor)
        {
            visitor.Visit(this);
        }

        public async Task Execute(BehaviorContext<OrderState, IOrderAccepted> context, Behavior<OrderState, IOrderAccepted> next)
        {
            Console.WriteLine("Hello, World!!!, order ID is {0}", context.Data.OrderId);
            await next.Execute(context).ConfigureAwait(false);
        }

        public Task Faulted<TException>(BehaviorExceptionContext<OrderState, IOrderAccepted, TException> context, Behavior<OrderState, IOrderAccepted> next) where TException : Exception
        {
            // clean up some properties here
            return next.Faulted(context);
        }

        public void Probe(ProbeContext context)
        {
            context.CreateScope("accept-order");
        }
    }
}
