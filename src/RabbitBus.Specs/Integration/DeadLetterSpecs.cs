using System.Collections.Generic;
using System.Text;
using ExpectedObjects;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue
	{
		const string SpecId = "4D610866-DEC9-4462-A2EA-2C9814AF9CE1";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx => ctx.WithDeadLetterQueue()
					                  	.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue();
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};

		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_publish_message_to_the_dead_letter_queue =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}

	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_with_headers_without_requeue
	{
		const string SpecId = "4D610866-DEC9-4462-A2EA-2C9814AF9CE1";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;
		static Dictionary<string, object> _headers;

		Establish context = () =>
			{
				_headers = new Dictionary<string, object>();
				_headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));

				_bus = new BusBuilder().Configure(ctx => ctx.WithDeadLetterQueue()
				                                         	.Consume<TestMessage>()
				                                         	.WithExchange(SpecId)
				                                         	.WithQueue(SpecId))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue();
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};

		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"), _headers);

		It should_publish_message_to_the_dead_letter_queue_with_headers_preserved =
			() => _deadLetterQueue.GetMessageProperties<TestMessage>().Headers.Keys.ShouldContain("header-key");
	}

	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue_with_custom_default_dead_letter_queue
	{
		const string SpecId = "6B49024F-367A-467E-9E45-3D8C5319E8A6";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.WithDeadLetterQueue()
				                                         	.WithDeadLetterQueue("custom-deadletter")
				                                         	.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue("custom-deadletter");
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};

		Cleanup after = () =>
			{
				_exchange.Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_publish_message_to_the_dead_letter_queue =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}


	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue_with_dead_letter_queue_unconfigured
	{
		const string SpecId = "4D610866-DEC9-4462-A2EA-2C9814AF9CE1";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue();
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};

		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_not_publish_message_to_the_dead_letter_queue =
			() => _deadLetterQueue.GetMessage<TestMessage>().ShouldBeNull();
	}
}