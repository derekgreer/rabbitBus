using System;
using System.Threading;
using Machine.Specifications;
using RabbitBus.Logging;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject(typeof (MessageContext<TestMessage>))]
	public class when_a_message_is_configured_without_auto_acknowledge
	{
		const string SpecId = "2F6E178C-CA80-45EF-865C-2DB8380FC1E9";
		static Bus _bus;
		static IConsumerContext<TestMessage> _consumerContext;
		static IMessageContext<TestMessage> _messageContext;
		static RabbitExchange _exchange;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>()
				                                         	.WithExchange(SpecId)
				                                         	.WithQueue(SpecId, cfg => cfg.Not.AutoAcknowledge())).Build();
				_bus.Connect();
				_consumerContext = _bus.CreateConsumerContext<TestMessage>();

				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
				_exchange.Publish(new TestMessage("test"));
				Thread.Sleep(1000);
			};

		Cleanup after = () =>
			{
				_bus.Close();
				new RabbitQueue(SpecId, SpecId).Empty().Delete().Close();
				_exchange.Close();
				_consumerContext.Dispose();
			};

		Because of = () => new Action(() => _messageContext = _consumerContext.GetMessage()).ExecuteUntil(() => _messageContext != null)();

		It should_set_acceptance_required_to_true = () => _messageContext.AcceptanceRequired.ShouldBeTrue();
	}

	[Integration]
	[Subject(typeof (MessageContext<TestMessage>))]
	public class when_a_message_is_configured_with_auto_acknowledge
	{
		const string SpecId = "F6258EC1-E15A-4CD2-8049-0BD9C7884F1E";
		static Bus _bus;
		static IConsumerContext<TestMessage> _consumerContext;
		static IMessageContext<TestMessage> _messageContext;
		static RabbitExchange _exchange;

		Establish context = () =>
			{
				Logger.UseLogger(new ConsoleLogger());

				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>()
				                                         	.WithExchange(SpecId)
				                                         	.WithQueue(SpecId, cfg => cfg.AutoAcknowledge())).Build();
				
				_bus.Connect();
				_consumerContext = _bus.CreateConsumerContext<TestMessage>();

				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);
				_exchange.Publish(new TestMessage("test"));
			};

		Cleanup after = () =>
			{
				_bus.Close();
				new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false, true, false, true).Delete().Close();
				_consumerContext.Dispose();
				_exchange.Close();
			};

		Because of = () => new Action(() => _messageContext = _consumerContext.GetMessage()).ExecuteUntil(() => _messageContext != null)();

		It should_set_acceptance_required_to_false = () => _messageContext.AcceptanceRequired.ShouldBeFalse();
	}
}