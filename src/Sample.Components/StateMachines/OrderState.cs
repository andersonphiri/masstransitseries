using Automatonymous;
using MassTransit.Saga;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Sample.Components.StateMachines
{
    public class OrderState : SagaStateMachineInstance,
        ISagaVersion
    {

        [BsonId]
        public Guid CorrelationId { get; set; }
        public string CurrentState { get; set; }
        public DateTimeOffset? Updated { get; set; }
        public string CustomerNumber { get; set; }
        public int Version { get; set; }
        public DateTimeOffset? SubmitDate { get; set; }
    }

}
