using Machine.Specifications;
using RabbitBus.Serialization.Json;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("JSON Serialization")]
	public class when_serializating_with_the_json_serializer
	{
		const string SpecId = "DFE0545B-C280-47E4-A565-1EBD48ED25F0";
		static int _deliveryMode;
		static RabbitQueue _queue;
		static IBasicProperties _properties;
		static Bus _bus;

		Cleanup after = () =>
		{
			_bus.Close();
			_queue.Delete().Close();
		};

		Establish context = () =>
		{
			_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId).WithSerializationStrategy(new JsonSerializationStrategy())).Build();
			_bus.Connect();
			_queue = new RabbitQueue(SpecId, SpecId);

			_bus.Publish(new TestMessage("test"));
		};

		Because of = () => _properties = _queue.GetMessageProperties<TestMessage>(new JsonSerializationStrategy());

		It should_set_the_content_type_to_json = () => _properties.ContentType.ShouldEqual("application/json; charset=utf-8");

		It should_set_the_content_encoding_to_utf8 = () => _properties.ContentEncoding.ShouldEqual("utf-8");
	}
}