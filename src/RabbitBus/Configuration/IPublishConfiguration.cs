using System;

namespace RabbitBus.Configuration
{
	public interface IPublishConfiguration
	{
		IPublishConfiguration Not { get; }
		IPublishConfiguration WithExchange(string exchangeName);
		IPublishConfiguration WithExchange(string exchangeName, Action<IExchangeConfiguration> exchangeConfiguration);
		IPublishConfiguration WithSerializationStrategy(ISerializationStrategy serializationStrategy);
		IPublishConfiguration WithDefaultRoutingKey(string routingKey);
		IPublishConfiguration Persistent();
		IPublishConfiguration Signed();
	}
}