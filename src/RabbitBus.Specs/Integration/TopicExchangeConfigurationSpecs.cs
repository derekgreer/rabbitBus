using System;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Topic Exchange")]
	public class when_configuring_a_message_with_a_topic_exchange
	{
		static string _exchangeType;
		static IPublishConfigurationContext _publishConfigurationContext;

		Establish context = () => new BusBuilder()
			.Configure(ctx => _publishConfigurationContext = ctx.Publish<TestMessage>().WithExchange(null, cfg => cfg.Topic()));

		It should_set_exchange_type_to_topic = () => ((IPublishInfoSource) _publishConfigurationContext).PublishInfo.ExchangeType.ShouldEqual(ExchangeType.Topic);
	}

	[Integration]
	[Subject("Topic Exchange")]
	public class when_subscribed_to_recieve_messages_with_topic_from_a_topic_exchange_publishing_matching_messages
	{
		const string SpecId = "4D467367-1E3E-44F3-BCCF-31846AEEE70A";
		const string TopicName = "test.this.topic";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static readonly TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_exchange = new RabbitExchange(SpecId, ExchangeType.Topic);
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Topic())
					.WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; }, "*.this.*");
			};

		Cleanup after = () => _bus.Close();

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"), TopicName)).BlockUntil(() => _actualMessage != null)();

		It should_receive_the_message = () => _actualMessage.ProvideDefault(() => _default).Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Topic Exchange")]
	public class when_subscribed_to_recieve_messages_from_a_topic_exchange_publishing_non_matching_messages
	{
		const string SpecId = "8913617F-149A-435E-B05D-17B249E0CF31";
		const string TopicName = "test.this.topic";
		const string OtherName = "test.other.topic";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_exchange = new RabbitExchange(SpecId, ExchangeType.Topic);
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Topic()).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; },
				                                "*.this.*");
			};

		Cleanup after = () => _bus.Close();

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"), OtherName)).BlockUntil(() => _actualMessage != null)();

		It should_not_receive_the_message = () => _actualMessage.ShouldBeNull();
	}

	[Integration]
	[Subject("Topic Exchange")]
	public class when_subscribed_to_recieve_messages_with_default_topic_from_a_topic_exchange_publishing_matching_messages
	{
		const string SpecId = "283564BA-77A6-47C5-BB73-47AAADC6F9B4";
		const string TopicName = "test.this.topic";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static readonly TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_exchange = new RabbitExchange(SpecId, ExchangeType.Topic);
				_bus = new BusBuilder().Configure(ctx => 
					ctx.WithLogger(new ConsoleLogger()).Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Topic()).WithDefaultRoutingKey("*.this.*").WithQueue(SpecId)).Build();
				_bus.Connect();
				// Routing key not specified
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; });
			};

		Cleanup after = () => _bus.Close();

		Because of = () => new Action(() => _exchange.Publish(new TestMessage("test"), TopicName)).BlockUntil(() => _actualMessage != null)();

		It should_receive_the_message = () => _actualMessage.ProvideDefault(() => _default).Text.ShouldEqual("test");
	}

	[Integration]
	[Subject(typeof (BusBuilder))]
	public class when_publishing_a_messagage_with_a_topic
	{
		const string SpecId = "A956C075-A4DF-47D3-B85A-949DE96C352D";
		const string TopicName = "test.this.topic";
		const string _expectedMessage = "test";
		static Bus _bus;
		static RabbitQueue _queue;
		static readonly TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Topic, SpecId, TopicName);
				_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId, cfg => cfg.Topic())).Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _bus.Publish(new TestMessage(_expectedMessage), TopicName);

		It should_be_received_by_subscribers_to_the_topic = () => _queue.GetMessage<TestMessage>().ProvideDefault(() => _default).Text.ShouldEqual(_expectedMessage);
	}

	[Integration]
	[Subject(typeof (BusBuilder))]
	public class when_publishing_a_messagage_with_a_default_topic
	{
		const string SpecId = "88624E57-7A55-49FC-8BBD-6B932B68A1CD";
		const string TopicName = "test.this.topic";
		const string _expectedMessage = "test";
		static Bus _bus;
		static RabbitQueue _queue;
		static readonly TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_queue = new RabbitQueue("localhost", SpecId, ExchangeType.Topic, SpecId, TopicName);

				_bus = new BusBuilder().Configure(ctx => ctx.Publish<TestMessage>().WithExchange(SpecId, cfg => cfg.Topic()).WithDefaultRoutingKey(TopicName)).Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_queue.Delete().Close();
			};

		Because of = () => _bus.Publish(new TestMessage(_expectedMessage));

		It should_be_received_by_subscribers_to_the_topic =
			() => _queue.GetMessage<TestMessage>().ProvideDefault(() => _default).Text.ShouldEqual(_expectedMessage);
	}
}