using MassTransit;
using MassTransit.Testing;
using NUnit.Framework;
using Sample.Components.Consumers;
using Sample.Contracts;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace Sample.Components.Tests
{
    [TestFixture]
    public class When_an_order_request_is_consumed
    {
        private const string customerNumber = "12345";
        private const string customerNumberFail = "TEST123456";

        [Test]
        public async Task Should_respond_with_acceptance_if_ok()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                var response = await requestClient.GetResponse<IOrderSubmissionAccepted>(new {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber

                });
                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                Assert.That(harness.Sent.Select<IOrderSubmissionAccepted>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }

        [Test]
        public async Task Should_respond_with_rejected_if_test_customer_number()
        {
            var harness = new InMemoryTestHarness();
            var consumer = harness.Consumer<SubmitOrderConsumer>();
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                var response = await requestClient.GetResponse<IOrderSubmissionRejected>(new
                {
                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumberFail

                });
                Assert.That(response.Message.OrderId, Is.EqualTo(orderId));
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                Assert.That(harness.Sent.Select<IOrderSubmissionRejected>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }
        }


        [Test]
        public async Task Should_consume_submit_order_commands()
        {
            var harness = new InMemoryTestHarness() { TestTimeout = TimeSpan.FromSeconds(5) };
            var consumer = harness.Consumer<SubmitOrderConsumer>();
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                await harness.InputQueueSendEndpoint.Send<ISubmitOrder>(new
                {

                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber
                });
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                Assert.That(harness.Sent.Select<IOrderSubmissionAccepted>().Any(), Is.False);
                Assert.That(harness.Sent.Select<IOrderSubmissionRejected>().Any(), Is.False);
            }
            finally
            {
                await harness.Stop();
            }

        }

        [Test]
        public async Task Should_not_publish_order_submitted_event_when_rejected()
        {
            var harness = new InMemoryTestHarness() { TestTimeout = TimeSpan.FromSeconds(5) };
            var consumer = harness.Consumer<SubmitOrderConsumer>();
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                await harness.InputQueueSendEndpoint.Send<ISubmitOrder>(new
                {

                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumberFail
                });
                Assert.That(consumer.Consumed.Select<ISubmitOrder>().Any(), Is.True);
                //Assert.That(harness.Sent.Select<IOrderSubmissionAccepted>().Any(), Is.False);
                //Assert.That(harness.Sent.Select<IOrderSubmissionRejected>().Any(), Is.False);
                Assert.That(harness.Published.Select<OrderSubmitted>().Any(), Is.False);
            }
            finally
            {
                await harness.Stop();
            }

        }


        [Test]
        public async Task Should_publish_order_submitted_event()
        {
            var harness = new InMemoryTestHarness() {  };
            var consumer = harness.Consumer<SubmitOrderConsumer>();
            await harness.Start();
            try
            {
                var orderId = NewId.NextGuid();
                var requestClient = await harness.ConnectRequestClient<ISubmitOrder>();
                await harness.InputQueueSendEndpoint.Send<ISubmitOrder>(new
                {

                    OrderId = orderId,
                    InVar.Timestamp,
                    CustomerNumber = customerNumber
                });
                Assert.That(harness.Published.Select<OrderSubmitted>().Any(), Is.True);
            }
            finally
            {
                await harness.Stop();
            }

        }

    }

}