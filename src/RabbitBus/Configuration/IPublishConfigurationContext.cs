using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitBus.Configuration
{
	public interface IPublishConfigurationContext
	{
		IPublishConfigurationContext WithExchange(string exchangeName);
		IPublishConfigurationContext WithExchange(string exchangeName, Action<IExchangeConfiguration> exchangeConfiguration);
		IPublishConfigurationContext WithSerializationStrategy(ISerializationStrategy serializationStrategy);
		IPublishConfigurationContext WithDefaultRoutingKey(string routingKey);
		IPublishConfigurationContext Persistent();
		IPublishConfigurationContext Signed();
		IPublishConfigurationContext OnReplyError(Action<IErrorContext> callback);
		IPublishConfigurationContext WithDefaultHeaders(IDictionary<string, object> headers);
		IPublishConfigurationContext WithExpiration(TimeSpan fromMilliseconds);
	}
}