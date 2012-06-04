using System;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Subscribing")]
	public class when_subscribing_to_receive_a_message_type_without_an_explicit_queue_configuration
	{
		public const string ExchangeName = "TestMessage";
		static string _actualMessage;
		static string _expectedMessage;
		static Bus _subject;
		static RabbitExchange _exchange;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", ExchangeName, ExchangeType.Direct);

				_subject = new BusBuilder()
					.Configure(ctx => ctx.WithLogger(new ConsoleLogger()).Consume<TestMessage>().WithExchange(ExchangeName))
					.Build();
				_subject.Connect();
				_subject.Subscribe<TestMessage>(m => { _actualMessage = m.Message.Text; });
				_expectedMessage = "test";
			};

		Cleanup after = () => _subject.Close();

		Because of = () => _exchange.Publish(new TestMessage("test"));

		It should_subscribe_with_the_default_registration_conventions =
			() => new Action(() => _actualMessage.ShouldEqual(_expectedMessage)).RepeatUntilSuccessfulOrTimeout();
	}

	[Integration]
	[Subject("Subscribing")]
	public class when_subscribing_to_receive_a_message_type_without_an_explicit_subscription_registration
	{
		public const string ExchangeName = "TestMessage";
		static string _actualMessage;
		static string _expectedMessage;
		static Bus _subject;
		static RabbitExchange _exchange;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", ExchangeName, ExchangeType.Direct);

				var builder = new BusBuilder();
				_subject = builder.Build();
				_subject.Connect();
				_subject.Subscribe<TestMessage>(m => { _actualMessage = m.Message.Text; });
				_expectedMessage = "test";
			};

		Cleanup after = () => _subject.Close();

		Because of = () => _exchange.Publish(new TestMessage("test"));

		It should_subscribe_with_the_default_registration_conventions =
			() => new Action(() => _actualMessage.ShouldEqual(_expectedMessage)).RepeatUntilSuccessfulOrTimeout();
	}
}