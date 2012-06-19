using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Persistent Messages")]
	public class when_configuring_a_message_to_be_persistent
	{
		const string SpecId = "FB49868F-C862-40F7-A018-666866003789";
		static RabbitQueue _queue;
		static IBasicProperties _message;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId).Persistent()).Build();
				_bus.Connect();
				_queue = new RabbitQueue(SpecId, SpecId);
				_bus.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _message = _queue.GetMessageProperties<TestMessage>(new BinarySerializationStrategy());

		It should_set_the_delivery_mode_to_persistent = () => _message.DeliveryMode.ShouldEqual((byte) 2);
	}
}