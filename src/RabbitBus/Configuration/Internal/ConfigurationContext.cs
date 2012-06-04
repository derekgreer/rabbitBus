using System;
using RabbitBus.Logging;

namespace RabbitBus.Configuration.Internal
{
	class ConfigurationContext : IConfigurationContext
	{
		public ConfigurationContext()
		{
			ConfigurationModel = new ConfigurationModel();
		}

		public IConfigurationModel ConfigurationModel { get; set; }

		public IPublishConfigurationContext Publish<TMessage>()
		{
			var p = new PublishConfigurationContext(typeof(TMessage).Name);
			ConfigurationModel.PublicationRouteConfiguration.AddPolicy<MappingRouteInfoLookupStrategy<IPublishInfo>>(
				typeof (TMessage), p.PublishInfo);
			return p;
		}

		public IConsumeConfigurationContext Consume<TMessage>()
		{
			var c = new ConsumeConfigurationContext(typeof(TMessage).Name);
			ConfigurationModel.ConsumeRouteConfiguration.AddPolicy<MappingRouteInfoLookupStrategy<IConsumeInfo>>(
				typeof (TMessage), c.ConsumeInfo);
			return c;
		}

		public IConfigurationContext WithDefaultSerializationStrategy(ISerializationStrategy serializationStrategy)
		{
			ConfigurationModel.DefaultSerializationStrategy = serializationStrategy;
			return this;
		}

		public IConfigurationContext WithLogger(ILogger logger)
		{
			Logger.UseLogger(logger);
			return this;
		}

		public IConfigurationContext WithDeadLetterQueue()
		{
			ConfigurationModel.DefaultDeadLetterStrategy = new DefaultDeadLetterStrategy();
			return this;
		}

		public IConfigurationContext WithDeadLetterQueue(string queueName)
		{
			ConfigurationModel.DefaultDeadLetterStrategy = new DefaultDeadLetterStrategy(queueName);
			return this;
		}

		public IConfigurationContext AutoSubscribe(IAutoSubscriptionModel autoSubscriptionModel)
		{
			var autoSubscriber = new AutoSubscriber();
			autoSubscriber.Subscribe(ConfigurationModel, autoSubscriptionModel);
			return this;
		}

		public IConfigurationContext WithConnectionUnavailableQueueStrategy(IQueueStrategy queueStrategy)
		{
			ConfigurationModel.ConnectionDownQueueStrategy = queueStrategy;
			return this;
		}

		public IConfigurationContext WithReconnectionAttemptInterval(TimeSpan timeSpan)
		{
			ConfigurationModel.ReconnectionInterval = timeSpan;
			return this;
		}
	}
}