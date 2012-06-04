using System.Text;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Default Serializaton")]
	public class when_configuring_the_default_serialization_strategy
	{
		const string SpecId = "3036A3FA-38E4-4A94-9D83-8B2100447FBD";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = "ǝ ƃ  ɐ s s ǝ ɯ  ʇ s ǝ ┴";
		const string RabbitMqHost = "localhost";
		static RabbitQueue _rabbitQueue;
		static TestSerializationStrategy _serializationStrategy;
		static Bus _bus;

		Establish context = () =>
			{
				_serializationStrategy = new TestSerializationStrategy(Encoding.UTF8.GetBytes(ExpectedMessage),
				                                                       ExpectedMessage + "X");
				_rabbitQueue = new RabbitQueue(RabbitMqHost, ExchangeName, ExchangeType.Fanout, QueueName);
				_bus = new BusBuilder().Configure(ctx => ctx.WithDefaultSerializationStrategy(_serializationStrategy)
				                                         	.Publish<string>().WithExchange(SpecId, cfg => cfg.Fanout())).Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_rabbitQueue.Close();
				_bus.Close();
			};

		Because of = () => _bus.Publish(SpecId);

		It should_use_the_configured_default_serialization_strategy_for_nonconfigured_messages =
			() => _rabbitQueue.GetMessage<string>(_serializationStrategy).ShouldEqual(ExpectedMessage + "X");
	}

	[Integration]
	[Subject(typeof (BusBuilder))]
	public class when_configuring_a_published_message_with_an_encoding_strategy
	{
		static string _exchangeType;
		static IPublishConfigurationContext _publishConfigurationContext;

		Establish context = () =>
			{
				var strategy = new TestSerializationStrategy(Encoding.UTF8.GetBytes("test"), "test");
				new BusBuilder().Configure(
					ctx => _publishConfigurationContext = ctx.Publish<TestMessage>().WithSerializationStrategy(strategy)).Build();
			};

		It should_set_the_publish_configuration_to_the_message_encoding_strategy =
			() => ((IPublishInfoSource) _publishConfigurationContext).PublishInfo.SerializationStrategy.ShouldBeOfType(typeof (TestSerializationStrategy));
	}


	[Integration]
	[Subject("Error Handling")]
	public class when_a_publishing_a_message_with_an_encoding_strategy
	{
		const string SpecId = "FB354593-6C98-4492-884D-4724697152FF";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = "ǝ ƃ  ɐ s s ǝ ɯ  ʇ s ǝ ┴";
		const string RabbitMqHost = "localhost";
		static Bus _bus;
		static RabbitQueue _rabbitQueue;
		static TestSerializationStrategy _serializationStrategy;

		Establish context = () =>
			{
				_serializationStrategy = new TestSerializationStrategy(Encoding.UTF8.GetBytes(ExpectedMessage),
				                                                       ExpectedMessage + "X");

				_bus = new BusBuilder().Configure(ctx => ctx.Publish<string>()
				                                         	.WithExchange(ExchangeName, cfg => cfg.Fanout())
				                                         	.WithSerializationStrategy(_serializationStrategy)).Build();
				_bus.Connect();
				_rabbitQueue = new RabbitQueue(RabbitMqHost, ExchangeName, ExchangeType.Fanout, QueueName);
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_rabbitQueue.Close();
			};

		Because of = () => _bus.Publish(SpecId);

		It should_publish_the_message_with_the_configured_serialization_strategy =
			() => _rabbitQueue.GetMessage<string>(_serializationStrategy).ShouldEqual(ExpectedMessage + "X");
	}

	public class TestSerializationStrategy : ISerializationStrategy
	{
		readonly object _deserialized;
		readonly byte[] _serialized;

		public TestSerializationStrategy(byte[] serialized, string deserialized)
		{
			_serialized = serialized;
			_deserialized = deserialized;
		}

		public string ContentType
		{
			get { return string.Empty; }
		}

		public string ContentEncoding
		{
			get { return string.Empty; }
		}

		public byte[] Serialize<T>(T message)
		{
			return _serialized;
		}

		public T Deserialize<T>(byte[] bytes)
		{
			return (T) _deserialized;
		}
	}
}