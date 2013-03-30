using System;
using System.Collections;

namespace RabbitBus.Utilities
{
	public interface IQueueDeclareContext
	{
		INegatableQueueDeclareContext Not { get; }
		IQueueDeclareContext AutoDelete();
		IQueueDeclareContext WithExchange(string exchangeName);
		IQueueDeclareContext WithExchange(string exchangeName, Action<IExchangeDeclareContext> configure);
		IQueueDeclareContext WithExpiration(TimeSpan expirationTimeSpan);
		IQueueDeclareContext WithDeadLetterExchange(string deadLetterExchange);
		IQueueDeclareContext WithDeadLetterExchange(string deadLetterExchange, string deadletterRoutingKey);
		IQueueDeclareContext WithRoutingKey(string routingKey);
		IQueueDeclareContext WithHeaders(IDictionary dictionary);
	}
}