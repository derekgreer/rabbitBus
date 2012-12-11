using System;
using System.Collections.Generic;

namespace RabbitBus.Configuration.Internal
{
	class ConfigurationModel : IConfigurationModel
	{
		public ConfigurationModel()
		{
			PublishRouteConfiguration = new RouteConfiguration<IPublishInfo>();
			PublishRouteConfiguration.AddStrategy<MappingRouteInfoLookupStrategy<IPublishInfo>>();
			PublishRouteConfiguration.AddStrategy<DefaultPublishRouteInfoLookupStrategy>();

			ConsumeRouteConfiguration = new RouteConfiguration<IConsumeInfo>();
			ConsumeRouteConfiguration.AddStrategy<MappingRouteInfoLookupStrategy<IConsumeInfo>>();
			ConsumeRouteConfiguration.AddStrategy<DefaultConsumeRouteInfoLookupStrategy>();
			DefaultSerializationStrategy = new BinarySerializationStrategy();
			DefaultDeadLetterStrategy = new NullDeadLetterStrategy();
			AutoSubscriptions = new List<AutoSubscription>();
			ConnectionDownQueueStrategy = new ThrowingQueueStrategy<ConnectionUnavailableException>();
			ReconnectionInterval = TimeSpan.FromSeconds(10);
			ReconnectionTimeout = TimeSpan.FromMinutes(60);
		}

		public IRouteConfiguration<IPublishInfo> PublishRouteConfiguration { get; set; }
		public IRouteConfiguration<IConsumeInfo> ConsumeRouteConfiguration { get; set; }
		public ISerializationStrategy DefaultSerializationStrategy { get; set; }
		public IDeadLetterStrategy DefaultDeadLetterStrategy { get; set; }
		public IList<AutoSubscription> AutoSubscriptions { get; set; }
		public IQueueStrategy ConnectionDownQueueStrategy { get; set; }
		public TimeSpan ReconnectionInterval { get; set; }
		public TimeSpan ReconnectionTimeout { get; set; }
	}
}