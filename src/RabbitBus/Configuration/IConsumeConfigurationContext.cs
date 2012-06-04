using System;

namespace RabbitBus.Configuration
{
	public interface IConsumeConfigurationContext
	{
		IConsumeConfigurationContext WithExchange(string exchangeName);
		IConsumeConfigurationContext WithExchange(string exchangeName, Action<IExchangeConfiguration> exchangeConfiguration);
		IConsumeConfigurationContext WithQueue(string queueName);
		IConsumeConfigurationContext WithQueue(string queueName, Action<IQueueConfiguration> exchangeConfiguration);
		IConsumeConfigurationContext WithDefaultRoutingKey(string routingKey);
		IConsumeConfigurationContext WithSerializationStrategy(ISerializationStrategy serializationStrategy);
		IConsumeConfigurationContext OnError(Action<IErrorContext> callback);
	}
}