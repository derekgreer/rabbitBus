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
		IConfigurationContext WithDeadLetterQueue();
		IConfigurationContext WithDeadLetterQueue(string queueName);
		IConfigurationContext WithConnectionUnavailableQueueStrategy(IQueueStrategy queueStrategy);
		IConfigurationContext WithReconnectionAttemptInterval(TimeSpan timeSpan);
	}
}