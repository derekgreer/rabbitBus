using System;
using ExpectedObjects;
using Machine.Specifications;
using Moq;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;
using It = Machine.Specifications.It;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Error Handling")]
	public class when_an_error_callback_is_configured_for_a_subscription
	{
		const string SpecId = "E0C560A5-77C9-476F-9D07-E1C2DB3EB3D4";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = SpecId;
		static bool _callbackInvoked;
		static RabbitExchange _rabbitExchange;
		static Bus _bus;

		Establish context = () =>
			{
				_rabbitExchange = new RabbitExchange("localhost", ExchangeName, ExchangeType.Fanout);

				_bus = new BusBuilder().Configure(ctx => ctx.WithDeadLetterQueue()
																								.WithDefaultSerializationStrategy(new ErrorSerializationStrategy())
																								.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout())
																								.WithQueue(QueueName)
																								.OnError(errorContext => { _callbackInvoked = true; })).Build();
			
				_bus.Connect();

				_bus.Subscribe(RabbitHandlers.EmptyHandler<TestMessage>());
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_rabbitExchange.Close();
			};

		Because of = () => new Action(() => _rabbitExchange.Publish(new TestMessage(ExpectedMessage))).BlockUntil(() => _callbackInvoked)();

		It should_invoke_callback_on_errors = () => _callbackInvoked.ShouldBeTrue();
	}

	[Integration]
	[Subject("Error Handling")]
	public class when_rejecting_message_without_requeuing_on_error_callback
	{
		const string SpecId = "9A560F22-4E64-440A-8684-376853E25779";
		const string ExpectedMessage = SpecId;
		static bool _callbackInvoked;
		static RabbitExchange _rabbitExchange;
		static Bus _bus;
		static RabbitQueue _rabbitQueue;

		Establish context = () =>
			{
				_rabbitExchange = new RabbitExchange("localhost", SpecId, ExchangeType.Fanout, true, false);
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Fanout, SpecId, true, false, true, false);

				_bus = new BusBuilder().Configure(ctx =>
					{
						ctx.WithDefaultSerializationStrategy(new ErrorSerializationStrategy());
						ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout().Durable().Not.AutoDelete())
							.WithQueue(SpecId, cfg => cfg.Durable().Not.AutoDelete())
							.OnError(errorContext =>
								{
									errorContext.RejectMessage(false);
									_bus.Close();
									_callbackInvoked = true;
								});
					}).Build();
				_bus.Connect();

				_bus.Subscribe(RabbitHandlers.EmptyHandler<TestMessage>());
			};

		Cleanup after = () =>
			{
				_bus.Close();
				new RabbitDeadLetterQueue().Empty().Close();
				_rabbitQueue.Delete().Close();
				_rabbitExchange.Delete().Close();
			};

		Because of = () => new Action(() => _rabbitExchange.Publish(new TestMessage(ExpectedMessage))).BlockUntil(() => _callbackInvoked)();

		It should_not_requeue_the_message = () => _rabbitQueue.GetMessage<TestMessage>().ShouldBeNull();
	}

	[Integration]
	[Subject("Error Handling")]
	public class when_an_error_occurs_with_the_default_behavior
	{
		const string SpecId = "6626DEB9-8576-44A3-A759-3FA27392FBDB";
		const string ExpectedMessage = SpecId;
		static bool _logMessageWritten;
		static RabbitExchange _rabbitExchange;
		static Bus _bus;
		static RabbitQueue _rabbitQueue;
		static RabbitDeadLetterQueue _deadLetterQueue;

		Establish context = () =>
			{
				var logSpy = new Mock<ILogger>();
				logSpy.Setup(x => x.Write(Moq.It.IsAny<LogEntry>()))
					.Callback<LogEntry>(e =>
						{
							if (e.Message.Contains("exception"))
							{
								_logMessageWritten = true;
								_bus.Close();
							}
						});

				_rabbitExchange = new RabbitExchange("localhost", SpecId, ExchangeType.Fanout, true, false);
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Fanout, SpecId, true, false, true, false);
				_deadLetterQueue = new RabbitDeadLetterQueue();

				_bus = new BusBuilder().Configure(ctx =>
					{
						ctx.WithLogger(logSpy.Object)
							.WithDefaultSerializationStrategy(new ErrorSerializationStrategy());
						ctx.Consume<TestMessage>()
							.WithExchange(SpecId, cfg => cfg.Fanout().Durable().Not.AutoDelete())
							.WithQueue(SpecId, cfg => cfg.Durable().Not.AutoDelete());
					}).Build();
				
				_bus.Connect();

				_bus.Subscribe(RabbitHandlers.EmptyHandler<TestMessage>());
			};

		Cleanup after = () =>
			{
				_rabbitExchange.Delete().Close();
				_rabbitQueue.Delete().Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of =
			() =>
			new Action(() => _rabbitExchange.Publish(new TestMessage(ExpectedMessage))).BlockUntil(() => _logMessageWritten)();

		It should_reject_without_requeuing_the_message =
			() => _rabbitQueue.GetMessage<TestMessage>(new BinarySerializationStrategy(), false).ShouldBeNull();

		It should_publish_the_rejected_message_to_the_dead_letter_queue =
			() => new TestMessage(ExpectedMessage).ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}
}