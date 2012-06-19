using Machine.Specifications;
using RabbitBus.Configuration;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;
using RabbitMQ.Client;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject(typeof (BusBuilder))]
	public class when_configuring_a_message_with_a_fanout_exchange
	{
		static IPublishConfigurationContext _publishConfigurationContext;

		Establish context = () => new BusBuilder().Configure(ctx => _publishConfigurationContext = ctx.Publish<TestMessage>().WithExchange("any", cfg => cfg.Fanout())).Build();

		It should_set_exchange_type_to_fanout = () => ((IPublishInfoSource) _publishConfigurationContext).PublishInfo.ExchangeType.ShouldEqual(ExchangeType.Fanout);
	}
}