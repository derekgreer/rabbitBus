using System;
using System.Collections.Generic;
using System.Text;
using ExpectedObjects;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitBus.Utilities;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue_with_configured_deadletter_exchange
	{
		const string SpecId = "D0B5C3B6-1040-4362-A014-979067900287";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx => ctx //.WithDeadLetterQueue()
						                  .Consume<TestMessage>()
						                  .WithExchange(SpecId)
						                  .WithQueue(SpecId)
						                  .WithDeadLetterExchange(DeadLetterQueue))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};


		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_publish_message_to_the_specified_dead_letter_exchange =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}


	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue_with_configured_deadletter_exchange_and_routing_key
	{
		const string SpecId = "2AD7B45D-5446-450F-BCC9-A697AE2B5AE0";
		const string DeadLetterQueue = SpecId + "-deadletter";
		const string DeadLetterRoutingKey = DeadLetterQueue;
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx => ctx.Consume<TestMessage>()
					                     .WithExchange(SpecId)
					                     .WithQueue(SpecId)
					                     .WithDeadLetterExchange(DeadLetterQueue, DeadLetterRoutingKey))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue("localhost", DeadLetterQueue, ExchangeType.Direct, DeadLetterQueue,
				                                             DeadLetterRoutingKey);
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};


		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Close();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_publish_message_to_the_specified_dead_letter_exchange =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}

	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_without_requeue
	{
		const string SpecId = "F33E1CA2-9F89-40E4-9752-448482E821D7";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx => ctx.Consume<TestMessage>()
					                     .WithDeadLetterExchange(DeadLetterQueue)
					                     .WithExchange(SpecId)
					                     .WithQueue(SpecId))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
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


	//  The following spec seems to indicate a bug in RabbitMQ as of version 3.0.4.
	// When configuring a queue with a dead letter exchange which has an expired message due to a per-message TTL,
	// the message is subsequently moved to the configured dead letter queue *with the same TTL* at which point it
	// lives out its TTL duration in the dead letter queue and is then removed from there as well.
	// Summary: Per-Message TTL + Native Dead Letter Exchanges = Incompatible Features

	/*
	[Integration]
	[Subject("Dead Letter")]
	public class when_a_message_configured_with_ttl_expires_in_queue_configured_with_deadletter_exchange
	{
		const string SpecId = "D60DF636-D1F6-45BA-82B8-7FFD3E3D8876";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
		{
			new QueueDeclarer()
			.WithName(SpecId)
			.WithExchange(SpecId, cfg => cfg.Direct().Not.AutoDelete().Durable())
			.WithDeadLetterQueue(DeadLetterQueue)
			.Declare();
			
			_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
			_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct, true, false);
		};

		Cleanup after = () =>
		{
			_exchange.Delete().Close();
			_bus.Close();
			_deadLetterQueue.Empty().Close();
		};

		Because of = () => _exchange.Publish(new TestMessage("expected message"), 10);

		It should_publish_message_to_the_dead_letter_queue =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}
	*/

	[Integration]
	[Subject("Dead Letter")]
	public class when_a_message_in_queue_configured_with_ttl_expires_in_queue_configured_with_deadletter_exchange
	{
		const string SpecId = "4D610866-DEC9-4462-A2EA-2C9814AF9CE1";
		const string DeadLetterExchange = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				Declare.Queue(SpecId, ctx =>
				                      ctx.Not.AutoDelete()
				                         .WithExpiration(TimeSpan.FromMilliseconds(1))
				                         .WithDeadLetterExchange(DeadLetterExchange)
				                         .WithExchange(SpecId));

				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterExchange);
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
			};

		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
				_deadLetterQueue.Empty().Delete();
			};

		Because of = () => _exchange.Publish(new TestMessage("expected message"));

		It should_publish_message_to_the_dead_letter_queue =
			() => new TestMessage("expected message").ToExpectedObject().ShouldEqual(_deadLetterQueue.GetMessage<TestMessage>());
	}


	[Integration]
	[Subject("Dead Letter")]
	public class when_rejecting_a_message_with_headers_without_requeue
	{
		const string SpecId = "849E69DF-9C32-4DCA-AC68-D0DACF5D1F3A";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;
		static Dictionary<string, object> _headers;

		Establish context = () =>
			{
				_headers = new Dictionary<string, object>();
				_headers.Add("header-key", Encoding.UTF8.GetBytes("header key value"));

				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>()
				                                            .WithDeadLetterExchange(DeadLetterQueue)
				                                            .WithExchange(SpecId)
				                                            .WithQueue(SpecId))
				                       .Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
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
	public class when_rejecting_a_message_without_requeue_with_custom_default_dead_letter_exchange
	{
		const string SpecId = "6B49024F-367A-467E-9E45-3D8C5319E8A6";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.WithDefaultDeadLetterExchange(DeadLetterQueue)
				                                            .Consume<TestMessage>()
				                                            .WithExchange(SpecId)
				                                            .WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
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
	public class when_rejecting_a_message_without_requeue_with_default_dead_letter_exchange
	{
		const string SpecId = "6B49024F-367A-467E-9E45-3D8C5319E8A6";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.WithDefaultDeadLetterExchange()
				                                            .Consume<TestMessage>()
				                                            .WithExchange(SpecId)
				                                            .WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue("deadletter");
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
		const string SpecId = "62A9F8DD-3325-477D-9B5E-892DE4B53903";
		const string DeadLetterQueue = SpecId + "-deadletter";
		static RabbitQueue _deadLetterQueue;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx
					                                         .Consume<TestMessage>()
					                                         .WithExchange(SpecId)
					                                         .WithQueue(SpecId))
				                       .Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => messageContext.RejectMessage(false));


				_deadLetterQueue = new RabbitDeadLetterQueue(DeadLetterQueue);
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