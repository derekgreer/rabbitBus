using System;
using System.Threading;
using Machine.Specifications;
using RabbitBus.Serialization.Json;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Publishing")]
	public class when_a_message_is_published_to_a_subscribed_queue
	{
		const string SpecId = "CF089B88-A48E-49C9-9D59-4AE64076E6FB";
		const string ExpectedMessage = SpecId;
		static Bus _bus;
		static string _actualMessage;
		static RabbitExchange _rabbitExchange;
		static IMessageContext<TestMessage> _messageContext;

		Establish context = () =>
			{
				_rabbitExchange = new RabbitExchange("localhost", SpecId, ExchangeType.Fanout);

				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout()).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext =>
					{
						_actualMessage = messageContext.Message.Text;
						_messageContext = messageContext;
						_messageContext.AcceptMessage();
					});
			};

		Cleanup after = () =>
			{
				_rabbitExchange.Delete().Close();
				_bus.Close();
			};

		Because of = () => new Action(() => _rabbitExchange.Publish(new TestMessage(ExpectedMessage))).BlockUntil(() => _actualMessage != null)();

		It should_invoke_subscription_callback = () => _actualMessage.ShouldEqual(ExpectedMessage);

		It should_contain_a_timestamp =
			() => _messageContext.TimeStamp.ShouldBeGreaterThan(new DateTime(1970, 1, 1, 0, 0, 0, 0));

		It should_contain_a_timestamp_of_type_datetime = () => _messageContext.TimeStamp.ShouldBeOfType<DateTime>();
	}

	[Integration]
	[Subject("Error Handling")]
	public class when_an_exception_occurs_during_receipt_of_a_message
	{
		const string SpecId = "9F5E466F-A154-4389-945F-69037975E5C1";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = SpecId;
		static Bus _bus;
		static string _actualMessage;
		static RabbitExchange _rabbitExchange;
		static Exception _exception;

		Establish context = () =>
			{
				_rabbitExchange = new RabbitExchange("localhost", ExchangeName, ExchangeType.Fanout);

				_bus = new BusBuilder().Configure(ctx => ctx
					.WithDefaultSerializationStrategy(new JsonSerializationStrategy())
					.Consume<TestMessage>().WithExchange(SpecId, cfg => cfg.Fanout()).WithQueue(SpecId)).Build();
				_bus.Connect();

				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message.Text; });

				AppDomain.CurrentDomain.UnhandledException += (sender, e) => _exception = (Exception) e.ExceptionObject;
			};

		Cleanup after = () =>
			{
				new RabbitDeadLetterQueue().Empty().Close();
				_rabbitExchange.Delete().Close();
				_bus.Close();
			};

		Because of = () => new Action(() => _rabbitExchange.Publish(new TestMessage(ExpectedMessage))).BlockUntil(() => _actualMessage != null)();

		It should_not_throw_an_exception = () => _exception.ShouldBeNull();
	}
}