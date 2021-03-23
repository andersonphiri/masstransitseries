using Automatonymous.Graphing;
using Automatonymous.Visualizer;
using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using Sample.Components.Consumers;
using Sample.Components.StateMachines;
using Sample.Contracts;
using System;
using System.Diagnostics;
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

        [Test]
        public async Task Should_cancel_when_customer_account_Closed()
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

                await harness.Bus.Publish<ICustomerAccountClosed>(new
                {
                    CustomerNumber = customerNumber,
                    CustomerId = InVar.Id
                });
                instanceId = await saga.Exists(orderId, x => x.Cancelled);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_accept_when_order_is_accepted()
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

                await harness.Bus.Publish<OrderAccepted>(new
                {
                    OrderId = orderId, Timestamp = InVar.Timestamp
                });
                instanceId = await saga.Exists(orderId, x => x.Accepted);
                Assert.That(instanceId, Is.Not.Null);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public void Show_me_the_state_machine()
        {
            var orderStateMachine = new OrderStateMachine();
            var graph = orderStateMachine.GetGraph();
            var generator = new StateMachineGraphvizGenerator(graph);
            string dots = generator.CreateDotFile();
            Console.WriteLine(dots);
            Debug.WriteLine(dots);
        }

    }


}
