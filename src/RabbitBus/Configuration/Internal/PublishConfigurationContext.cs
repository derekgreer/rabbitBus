using System;
using System.Collections;
using RabbitMQ.Client;

namespace RabbitBus.Configuration.Internal
{
	class PublishConfigurationContext : IPublishConfigurationContext, IPublishInfoSource
	{
		public PublishConfigurationContext() : this("default")
		{
		}

		public PublishConfigurationContext(string defaultName)
		{
			PublishInfo = new PublishInfo();
			PublishInfo.ExchangeName = defaultName;
		}

		public IPublishConfigurationContext WithExchange(string exchangeName)
		{
			return WithExchange(exchangeName, x => { });
		}

		public IPublishConfigurationContext WithExchange(string exchangeName,
		                                                 Action<IExchangeConfiguration> exchangeConfiguration)
		{
			PublishInfo.ExchangeName = exchangeName;
			var exchangeInfo = new ExchangeInfo();
			exchangeConfiguration(exchangeInfo);
			PublishInfo.IsDurable = exchangeInfo.IsDurable;
			PublishInfo.IsAutoDelete = exchangeInfo.IsAutoDelete;
			PublishInfo.ExchangeType = exchangeInfo.ExchangeType;
			return this;
		}

		public IPublishConfigurationContext WithSerializationStrategy(ISerializationStrategy serializationStrategy)
		{
			PublishInfo.SerializationStrategy = serializationStrategy;
			return this;
		}

		public IPublishConfigurationContext WithDefaultRoutingKey(string routingKey)
		{
			PublishInfo.DefaultRoutingKey = routingKey;
			return this;
		}

		public IPublishConfigurationContext Persistent()
		{
			PublishInfo.IsPersistent = true;
			return this;
		}

		public IPublishConfigurationContext Signed()
		{
			PublishInfo.IsSigned = true;
			return this;
		}

		public IPublishConfigurationContext OnReplyError(Action<IErrorContext> callback)
		{
			PublishInfo.ReplyInfo.ErrorCallback = callback;
			return this;
		}

		public IPublishConfigurationContext WithDefaultHeaders(IDictionary headers)
		{
			PublishInfo.DefaultHeaders = headers;
			return this;
		}


		public IPublishInfo PublishInfo { get; private set; }

		public IPublishConfigurationContext Fanout()
		{
			PublishInfo.ExchangeType = ExchangeType.Fanout;
			return this;
		}
	}
}