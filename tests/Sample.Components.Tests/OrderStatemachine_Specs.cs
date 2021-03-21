using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using Sample.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Components.Tests
{
   
    [TestFixture]
    public class Submitting_an_order
    {
        private const string customerNumber = "12345";
        private const string customerNumberFail = "TEST123456";
        [Test]
        public async Task Should_create_state_machine_instance()
        {
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState,OrderStateMachine>(orderStateMachine);
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmitted>(new {

                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber

                });
                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);
                await Task.Delay(1000);
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);
                var instance = saga.Sagas.Contains(instanceId.Value);
                Assert.That(instance.CustomerNumber, Is.EqualTo(customerNumber));
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_respond_to_status_checks()
        {
            var harness = new InMemoryTestHarness();
            var orderStateMachine = new OrderStateMachine();
            var saga = harness.StateMachineSaga<OrderState, OrderStateMachine>(orderStateMachine);
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                await harness.Bus.Publish<OrderSubmitted>(new
                {

                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber

                });
                Assert.That(saga.Created.Select(x => x.CorrelationId == orderId).Any(), Is.True);
                var instanceId = await saga.Exists(orderId, x => x.Submitted);
                Assert.That(instanceId, Is.Not.Null);
                var requestClient = await harness.ConnectRequestClient<CheckOrder>();
                var response = await requestClient.GetResponse<OrderStatus>(new
                {
                    OrderId = orderId
                });
                Assert.That(response.Message.State, Is.EqualTo(orderStateMachine.Submitted.Name));
            }
            finally
            {
                await harness.Stop();
            }
        }
    }
    
}
