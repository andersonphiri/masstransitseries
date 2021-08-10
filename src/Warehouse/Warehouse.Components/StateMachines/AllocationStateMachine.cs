

namespace Warehouse.Components.StateMachines
{
    using System;
    using Automatonymous;
    using Contracts;
    using MassTransit;
    using Microsoft.Extensions.Logging;

    public class AllocationStateMachine : MassTransitStateMachine<AllocationState>
    {
        public AllocationStateMachine()
        {
            Event(() => AllocationCreated, x => x.CorrelateById(m => m.Message.AllocationId));

            Schedule(() => HoldExpiration, x => x.HoldDurationToken, s =>
            {
                s.Delay = TimeSpan.FromHours(1);
                s.Received = x => x.CorrelateById(m => m.Message.AllocationId);
            });
            InstanceState(x => x.CurrentState);
            Initially(
                When(AllocationCreated)
                    .Schedule(HoldExpiration, context => context.Init<AllocationHoldDurationExpired>(new { context.Data.AllocationId }),
                        context => context.Data.HoldDuration)
                    .TransitionTo(Allocated)
                  //  ,When(ReleaseRequested)
                  //  .TransitionTo(Released)
            );

            During(
                Allocated, 
                When(HoldExpiration.Received)
                .ThenAsync(context => Console.Out.WriteLineAsync($"Allocation was released: {context.Instance.CorrelationId}"))
               // .TransitionTo(Released)
               .Finalize()
                );

            SetCompletedWhenFinalized();
        }
        #region Allocated state
        public Schedule<AllocationState, AllocationHoldDurationExpired> HoldExpiration { get; set; }
        public State Allocated { get; set; }
        public State Released { get; set; }
        public Event<AllocationCreated> AllocationCreated { get;  set; }

        #endregion
    }

}
