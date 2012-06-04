using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Publishing")]
	public class when_publishing_a_message_without_an_explicit_message_registration
	{
		public const string ExchangeName = "TestMessage";
		public const string QueueName = "TestMessage";
		static RabbitQueue _queue;
		static string _expectedMessage;
		static Bus _subject;

		Establish context = () =>
			{
				_queue = new RabbitQueue("localhost", ExchangeName, ExchangeType.Direct, QueueName);
				_expectedMessage = ExchangeName;
				_subject = new BusBuilder().Build();
				_subject.Connect();
			};

		Cleanup after = () =>
			{
				_subject.Close();
				_queue.Delete().Close();
			};

		Because of = () => _subject.Publish(new TestMessage(_expectedMessage));

		It should_publish_the_message_with_the_default_registration_conventions =
			() => _queue.GetMessage<TestMessage>().Text.ShouldEqual(_expectedMessage);
	}

	[Integration]
	[Subject("Publishing")]
	public class when_publishing_a_message_without_an_explicit_exchange_configuration
	{
		public const string ExchangeName = "TestMessage";
		public const string QueueName = "TestMessage";
		static RabbitQueue _queue;
		static string _expectedMessage;
		static Bus _subject;

		Establish context = () =>
			{
				_queue = new RabbitQueue("localhost", ExchangeName, ExchangeType.Direct, QueueName);
				_expectedMessage = ExchangeName;
				_subject = new BusBuilder()
					.Configure(ctx => ctx
						.WithLogger(new ConsoleLogger())
						.Publish<TestMessage>())
					.Build();
				_subject.Connect();
			};

		Cleanup after = () =>
			{
				_subject.Close();
				_queue.Delete().Close();
			};

		Because of = () => _subject.Publish(new TestMessage(_expectedMessage));

		It should_publish_the_message_with_the_default_registration_conventions =
			() => _queue.GetMessage<TestMessage>().Text.ShouldEqual(_expectedMessage);
	}
}