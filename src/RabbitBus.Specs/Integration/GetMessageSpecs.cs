using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Getting Messages")]
	public class when_getting_a_published_message
	{
		const string SpecId = "A09DFECB-53DC-4135-BFFB-81B775998684";
		const string ExchangeName = SpecId;
		const string QueueName = SpecId;
		const string ExpectedMessage = SpecId;
		static Bus _bus;
		static TestMessage _actualMessage;
		static RabbitExchange _exchange;
		static IConsumerContext<TestMessage> _consumerContext;
		static IMessageContext<TestMessage> _messageContext;

		Establish context = () =>
			{
				new RabbitQueue("localhost", ExchangeName, ExchangeType.Direct, QueueName, false, false, false, false).Close();

				_bus = new BusBuilder().Configure(ctx =>
				                                  ctx.Consume<TestMessage>()
				                                  	.WithExchange(SpecId, cfg => cfg.Not.AutoDelete())
				                                  	.WithQueue(SpecId, cfg => cfg.Not.AutoDelete())).Build();
				_bus.Connect();

				_exchange = new RabbitExchange("localhost", ExchangeName, ExchangeType.Direct, false, false);
				_exchange.Publish(new TestMessage(ExpectedMessage));

				_consumerContext = _bus.CreateConsumerContext<TestMessage>();
			};

		Cleanup after = () =>
			{
				_messageContext.AcceptMessage();
				_consumerContext.Dispose();
				new RabbitQueue("localhost", ExchangeName, ExchangeType.Direct, QueueName, false, false, false, false).Delete().
					Close();
				_exchange.Delete().Close();
				_bus.Close();
			};

		Because of = () => new Action(() => _messageContext = _consumerContext.GetMessage()).ExecuteUntil(() => _messageContext != null)();

		It should_retrieve_the_message = () => _messageContext.Message.Text.ShouldEqual(ExpectedMessage);
	}

	[Integration]
	[Subject("Getting Messages")]
	public class when_getting_all_published_messages
	{
		const string SpecId = "F2DE2364-C233-4B83-BE58-DBE882F110CC";
		static Bus _bus;
		static IList<TestMessage> _actualMessages = new List<TestMessage>();
		static RabbitExchange _exchange;

		Establish context = () =>
			{
				new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false, false, false, false).Close();

				_bus = new BusBuilder().Configure(ctx =>
																					ctx.Consume<TestMessage>()
																						.WithExchange(SpecId, cfg => cfg.Not.AutoDelete())
																						.WithQueue(SpecId, cfg => cfg.Not.AutoDelete())).Build();
				_bus.Connect();
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct, false, false);
				Enumerable.Range(0, 100).ToList().ForEach(i => _exchange.Publish(new TestMessage("test")));
			};

		Cleanup after = () =>
			{
				new RabbitQueue("localhost", SpecId, ExchangeType.Direct, SpecId, false, false, false, false).Delete().Close();
				_exchange.Delete().Close();
				_bus.Close();
			};

		Because of = () => _actualMessages = _bus.GetMessages<TestMessage>().ToList();

		It should_retrieve_all_published_messages = () => _actualMessages.Count().ShouldEqual(100);
	}
}