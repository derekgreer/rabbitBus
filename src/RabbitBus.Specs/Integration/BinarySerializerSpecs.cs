using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Binary Serialization")]
	public class when_serializating_with_the_binary_serializer
	{
		const string SpecId = "DFE0545B-C280-47E4-A565-1EBD48ED25F0";
		static RabbitQueue _queue;
		static IBasicProperties _properties;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId)).Build();
				_bus.Connect();
				_queue = new RabbitQueue(SpecId, SpecId);

				_bus.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _properties = _queue.GetMessageProperties<TestMessage>(new BinarySerializationStrategy());

		It should_set_the_content_type_to_dotnet_serialized_object =
			() => _properties.ContentType.ShouldEqual("application/x-dotnet-serialized-object");
	}
}