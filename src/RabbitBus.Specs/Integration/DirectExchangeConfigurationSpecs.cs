using System;
using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject(typeof (BusBuilder))]
	public class when_configuring_a_message_with_a_direct_exchange
	{
		static IPublishConfigurationContext _publishConfigurationContext;

		Establish context = () => new BusBuilder().Configure(ctx => _publishConfigurationContext = ctx.Publish<TestMessage>().WithExchange("any", cfg => cfg.Direct())).Build();

		It should_set_exchange_type_to_direct =
			() => ((IPublishInfoSource) _publishConfigurationContext).PublishInfo.ExchangeType.ShouldEqual(ExchangeType.Direct);
	}

	[Integration]
	[Subject("Topic Exchange")]
	public class when_subscribed_to_recieve_messages_with_a_routing_key_from_a_direct_exchange_publishing_matching_messages
	{
		const string SpecId = "90FB9404-ADAC-4005-A41E-D58E0A77F937";
		const string RoutingKey = "SomeName";
		static RabbitExchange _exchange;
		static Bus _bus;
		static TestMessage _actualMessage;
		static readonly TestMessage _default = new TestMessage("error");

		Establish context = () =>
			{
				_exchange = new RabbitExchange("localhost", SpecId, ExchangeType.Direct);

				_bus = new BusBuilder().Configure(ctx => ctx.Consume<TestMessage>().WithExchange(SpecId).WithQueue(SpecId)).Build();
				_bus.Connect();
				_bus.Subscribe<TestMessage>(messageContext => { _actualMessage = messageContext.Message; }, RoutingKey);
			};

		Cleanup after = () =>
			{
				_exchange.Delete().Close();
				_bus.Close();
			};

		Because of =
			() =>
			new Action(() => _exchange.Publish(new TestMessage("test"), RoutingKey)).BlockUntil(() => _actualMessage != null)();

		It should_receive_the_message = () => _actualMessage.ProvideDefault(() => _default).Text.ShouldEqual("test");
	}
}