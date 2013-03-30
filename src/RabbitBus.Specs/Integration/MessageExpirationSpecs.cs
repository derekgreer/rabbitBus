using System;
using System.Collections.Generic;
using System.Threading;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitBus.Utilities;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Expiration")]
	public class when_a_message_expires_from_a_queue_with_message_level_expiration
	{
		const string SpecId = "8C24F401-DDAA-4DA3-9901-4FAED4C2EB3C";
		static Bus _bus;
		static RabbitExchange _exchange;
		static Dictionary<string, object> _args;
		static RabbitQueue _queue;

		Establish context = () =>
			{
				Declare.Queue(SpecId, ctx => ctx.Not.AutoDelete().WithExchange(SpecId));

				_bus = new BusBuilder()
					.Configure(ctx =>
					           ctx.Publish<TestMessage>()
					              .WithExpiration(TimeSpan.FromMilliseconds(1))
					              .WithExchange(SpecId))
					.Build();

				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Close();
			};

		Because of = () =>
			{
				_bus.Publish(new TestMessage("test"));
				// give published message time to expire
				// As of version 3.0.4, the RabbitMQ server doesn't seem real reliable about
				// expiring messages in the stated time frame.  Consumers seem capable of consuming
				// messages after the time they should be expired.
				Thread.Sleep(1000);
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false,
										 true, false, false, string.Empty, null, null);
			};

		It should_remove_the_message_from_the_queue = () => _queue.GetMessage<TestMessage>().ShouldBeNull();
	}
}