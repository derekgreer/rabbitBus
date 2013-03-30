using System;

namespace RabbitBus.Configuration
{
	public interface IConsumeInfo
	{
		string ExchangeName { get; set; }
		string QueueName { get; set; }
		string DefaultRoutingKey { get; set; }
		bool IsQueueExclusive { get; set; }
		bool IsAutoAcknowledge { get; set; }
		bool IsQueueAutoDelete { get; set; }
		bool IsExchangeAutoDelete { get; set; }
		bool IsQueueDurable { get; set; }
		bool IsExchangeDurable { get; set; }
		string ExchangeType { get; set; }
		ushort QualityOfService { get; set; }
		ISerializationStrategy SerializationStrategy { get; set; }
		Action<IErrorContext> ErrorCallback { get; set; }
		IDeadLetterConfiguration DeadLetterConfiguration { get; set; }
		TimeSpan? Expiration { get; set; }
	}
}