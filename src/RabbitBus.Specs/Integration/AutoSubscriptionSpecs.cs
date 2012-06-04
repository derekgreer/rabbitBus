using System;
using System.Reflection;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Auto-subscribe")]
	public class when_auto_subscribing_to_a_message
	{
		const string SpecId = "86E10BFC-878F-404B-982B-EC18926BF33C";
		static string _messageText;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", "AutoMessage", ExchangeType.Direct, false, true);

				_bus = new BusBuilder()
					.Configure(ctx => ctx
					                  	.AutoSubscribe(new AutoSubscriptionModelBuilder()
					                  	               	.WithAssembly(Assembly.GetExecutingAssembly())
					                  	               	.WithSubscriptionConvention(new TestSubscriptionConvention())
																							.WithHandlerConvention(new DefaultHandlerConvention())
					                  	               	.Build())).Build();
				_bus.Connect();
			};

		Cleanup after = () =>
			{
				_bus.Close();
				_exchange.Close();
			};

		Because of = () => new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published = () => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}

	[Integration]
	[Subject("Auto-subscribe")]
	public class when_auto_subscribing_to_a_message_with_default_subscription_convention
	{
		const string SpecId = "86E10BFC-878F-404B-982B-EC18926BF33C";
		static string _messageText;
		static RabbitExchange _exchange;
		static Bus _bus;

		Establish context = () =>
		{
			_exchange = new RabbitExchange("localhost", "AutoMessage", ExchangeType.Direct, false, true);

			_bus = new BusBuilder()
				.Configure(ctx => ctx
														.AutoSubscribe(new AutoSubscriptionModelBuilder()
																						.WithAssembly(Assembly.GetExecutingAssembly())
																						.WithSubscriptionConvention(new DefaultSubscriptionConvention())
																						.WithHandlerConvention(new DefaultHandlerConvention())
																						.Build())).Build();
			_bus.Connect();
		};

		Cleanup after = () =>
		{
			_bus.Close();
			_exchange.Close();
		};

		Because of = () => new Action(() => _exchange.Publish(new AutoMessage("test"))).BlockUntil(() => AutoMessageHandler.Message != null)();

		It should_receive_the_message_when_published = () => AutoMessageHandler.Message.ProvideDefault().Text.ShouldEqual("test");
	}
}