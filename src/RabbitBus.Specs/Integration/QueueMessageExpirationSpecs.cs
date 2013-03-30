using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Expiration")]
	public class when_a_message_expires_from_a_queue_with_queue_level_expiration
	{
		const string SpecId = "E37C3DD2-7C3A-428A-9409-D849B9ECDC05";
		static Bus _bus;
		static RabbitExchange _exchange;
		static Dictionary<string, object> _args;
		static RabbitQueue _rabbitQueue;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx =>
					           ctx.Consume<TestMessage>()
					              .WithExchange(SpecId)
					              .WithQueue(SpecId, cfg =>
					                                 cfg.WithExpiration(TimeSpan.FromMilliseconds(1))
					                                    .Not.AutoDelete()))
					.Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(mc => { });
				_bus.Close();

				_exchange = new RabbitExchange(SpecId);

				_args = new Dictionary<string, object>();
				_args.Add("x-message-ttl", 1);
			};

		Cleanup after = () =>
			{
				_rabbitQueue.Close();
				_exchange.Close();
			};

		Because of = () =>
			{
				_exchange.Publish(new TestMessage("test"));
				Thread.Sleep(1000); // give published message time to expire
				_rabbitQueue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false,
											   true, false, false, string.Empty, null, _args);
			};

		It should_remove_the_message_from_the_queue = () => _rabbitQueue.GetMessage<TestMessage>().ShouldBeNull();
	}
}