using System;
using RabbitBus.Logging;

namespace RabbitBus.Configuration
{
	public interface IConfigurationContext
	{
		IPublishConfigurationContext Publish<TMessage>();
		IConsumeConfigurationContext Consume<TMessage>();
		IConfigurationContext WithDefaultSerializationStrategy(ISerializationStrategy serializationStrategy);
		IConfigurationContext WithLogger(ILogger logger);
		IConfigurationContext WithConnectionUnavailableQueueStrategy(IQueueStrategy queueStrategy);
		IConfigurationContext WithReconnectionAttemptInterval(TimeSpan timeSpan);
		IConfigurationContext WithReconnectionAttemptTimeout(TimeSpan timeSpan);
		IConfigurationContext WithDefaultDeadLetterExchange();
		IConfigurationContext WithDefaultDeadLetterExchange(string deadLetterExchangeName);
		IConfigurationContext WithDefaultDeadLetterExchange(string deadLetterExchangeName, string routingKey);
	}
}