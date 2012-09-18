using Machine.Specifications;
using Newtonsoft.Json;
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
		static RabbitQueue _queue;
		static IBasicProperties _properties;
		static Bus _bus;

		Establish context = () =>
			{
				_bus =
					new BusBuilder().Configure(
						ctx => ctx.Publish<TestMessage>().WithExchange(SpecId).WithSerializationStrategy(new JsonSerializationStrategy()))
						.Build();
				_bus.Connect();
				_queue = new RabbitQueue(SpecId, SpecId);

				_bus.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _properties = _queue.GetMessageProperties<TestMessage>(new JsonSerializationStrategy());

		It should_set_the_content_type_to_json = () => _properties.ContentType.ShouldEqual("application/json; charset=utf-8");

		It should_set_the_content_encoding_to_utf8 = () => _properties.ContentEncoding.ShouldEqual("utf-8");
	}

	[Integration]
	[Subject("JSON Serialization")]
	public class when_deserializating_to_protected_members_with_the_json_serializer
	{
		const string SpecId = "DFE0545B-C280-47E4-A565-1EBD48ED25F0";
		static RabbitQueue _queue;
		static Bus _bus;
		static TestMessage _message;
		static JsonSerializerSettings _settings;

		Establish context = () =>
			{
				_bus = new BusBuilder()
					.Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId)
					                  	.WithSerializationStrategy(new JsonSerializationStrategy())).Build();
				_bus.Connect();
				_queue = new RabbitQueue(SpecId, SpecId);

				_bus.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _message = _queue.GetMessage<TestMessage>(new JsonSerializationStrategy());

		It should_deserialize_to_protected_properties = () => _message.Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("JSON Serialization")]
	public class when_serializating_types_with_protected_members_with_the_json_serializer
	{
		const string SpecId = "DFE0545B-C280-47E4-A565-1EBD48ED25F0";
		static RabbitQueue _queue;
		static Bus _bus;
		static TestMessageWithProtectedTypes _message;
		static JsonSerializerSettings _settings;

		Establish context = () =>
		{
			_bus = new BusBuilder()
				.Configure(ctx => ctx.Publish<TestMessageWithProtectedTypes>().WithExchange(SpecId)
														.WithSerializationStrategy(new JsonSerializationStrategy())).Build();
			_bus.Connect();
			_queue = new RabbitQueue(SpecId, SpecId);

			_bus.Publish(new TestMessageWithProtectedTypes("test"));
		};

		Cleanup after = () =>
		{
			_bus.Close();
			_queue.Delete().Close();
		};

		Because of = () => _message = _queue.GetMessage<TestMessageWithProtectedTypes>(new JsonSerializationStrategy());

		It should_not_serialize_the_protected_properties = () => _message.GetText().ShouldBeNull();
	}
}