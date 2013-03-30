using System;
using System.Collections;
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
	public class when_a_message_expires_from_a_queue_with_message_instance_expiration
	{
		const string SpecId = "EDC22C3C-C23A-43CF-9A69-A1BFA7811896";
		static Bus _bus;
		static RabbitQueue _queue;

		Establish context = () =>
			{
				Declare.Queue(SpecId, ctx => ctx.Not.AutoDelete().WithExchange(SpecId));
				_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId)).Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Close();
			};

		Because of = () =>
			{
				_bus.Publish(new TestMessage("test"), new MessageProperties { Expiration = TimeSpan.FromMilliseconds(1)});
				Thread.Sleep(1000);
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false,
				                         true, false, false, string.Empty, null, null);
			};

		It should_remove_the_message_from_the_queue = () => _queue.GetMessage<TestMessage>().ShouldBeNull();
	}
}