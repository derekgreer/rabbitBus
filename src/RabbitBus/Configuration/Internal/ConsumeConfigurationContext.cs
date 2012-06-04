using System;

namespace RabbitBus.Configuration.Internal
{
	class ConsumeConfigurationContext : IConsumeConfigurationContext
	{
		public ConsumeConfigurationContext() : this("default")
		{
		}

		public ConsumeConfigurationContext(string defaultName)
		{
			ConsumeInfo = new ConsumeInfo();
			ConsumeInfo.ExchangeName = defaultName;
			ConsumeInfo.QueueName = defaultName;
		}

		public ISerializationStrategy SerializationStrategy { get; private set; }

		public IConsumeConfigurationContext WithExchange(string exchangeName)
		{
			return WithExchange(exchangeName, x => { });
		}

		public IConsumeConfigurationContext WithExchange(string exchangeName,
		                                                 Action<IExchangeConfiguration> exchangeConfiguration)
		{
			ConsumeInfo.ExchangeName = exchangeName;
			var exchangeInfo = new ExchangeInfo();
			exchangeConfiguration(exchangeInfo);
			ConsumeInfo.IsExchangeDurable = exchangeInfo.IsDurable;
			ConsumeInfo.IsExchangeAutoDelete = exchangeInfo.IsAutoDelete;
			ConsumeInfo.ExchangeType = exchangeInfo.ExchangeType;
			return this;
		}

		public IConsumeConfigurationContext WithQueue(string queueName)
		{
			return WithQueue(queueName, x => { });
		}

		public IConsumeConfigurationContext WithQueue(string queueName, Action<IQueueConfiguration> exchangeConfiguration)
		{
			ConsumeInfo.QueueName = queueName;
			var queueInfo = new QueueInfo();
			exchangeConfiguration(queueInfo);
			ConsumeInfo.IsQueueDurable = queueInfo.IsDurable;
			ConsumeInfo.IsQueueAutoDelete = queueInfo.IsAutoDelete;
			ConsumeInfo.IsAutoAcknowledge = queueInfo.IsAutoAcknowledge;
			ConsumeInfo.QualityOfService = queueInfo.QualityOfService;
			return this;
		}

		public IConsumeConfigurationContext WithDefaultRoutingKey(string routingKey)
		{
			ConsumeInfo.DefaultRoutingKey = routingKey;
			return this;
		}

		public IConsumeConfigurationContext WithSerializationStrategy(ISerializationStrategy serializationStrategy)
		{
			ConsumeInfo.SerializationStrategy = serializationStrategy;
			return this;
		}

		public IConsumeConfigurationContext OnError(Action<IErrorContext> callback)
		{
			ConsumeInfo.ErrorCallback = callback;
			return this;
		}

		public IConsumeInfo ConsumeInfo { get; private set; }
	}
}