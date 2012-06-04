using System;

namespace RabbitBus.Configuration
{
	public interface IConsumeConfiguration
	{
		IConsumeConfiguration WithExchange(string exchangeName);
		IConsumeConfiguration WithExchange(string exchangeName, Action<IExchangeConfiguration> exchangeConfiguration);
		IConsumeConfiguration WithQueue(string queueName);
		IConsumeConfiguration WithQueue(string queueName, Action<QueueInfo> exchangeConfiguration);
		IConsumeConfiguration WithDefaultRoutingKey(string routingKey);
		IConsumeConfiguration OnError(Action<IErrorContext> callback);
	}
}