using Automatonymous;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Warehouse.Components.StateMachines
{
    public class AllocationState : SagaStateMachineInstance, ISagaVersion
    {
        public  Guid? HoldDurationToken { get; set; }

        [BsonId]
        public Guid CorrelationId { get ; set ; }
        public string CurrentState { get; set; }
        public int Version { get ; set; }
    }
}
