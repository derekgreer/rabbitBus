using System;
using System.Collections.Generic;

namespace RabbitBus.Configuration.Internal
{
	class ConfigurationModel : IConfigurationModel
	{
		public ConfigurationModel()
		{
			PublicationRouteConfiguration = new RouteConfiguration<IPublishInfo>();
			PublicationRouteConfiguration.AddStrategy<MappingRouteInfoLookupStrategy<IPublishInfo>>();
			PublicationRouteConfiguration.AddStrategy<DefaultPublicationRouteInfoLookupStrategy>();

			ConsumeRouteConfiguration = new RouteConfiguration<IConsumeInfo>();
			ConsumeRouteConfiguration.AddStrategy<MappingRouteInfoLookupStrategy<IConsumeInfo>>();
			ConsumeRouteConfiguration.AddStrategy<DefaultSubscriptionRouteInfoLookupStrategy>();
			DefaultSerializationStrategy = new BinarySerializationStrategy();
			DefaultDeadLetterStrategy = new NullDeadLetterStrategy();
			AutoSubscriptions = new List<AutoSubscription>();
			ConnectionDownQueueStrategy = new ThrowingQueueStrategy<ConnectionUnavailableException>();
			ReconnectionInterval = TimeSpan.FromSeconds(10);
		}

		public IRouteConfiguration<IPublishInfo> PublicationRouteConfiguration { get; set; }
		public IRouteConfiguration<IConsumeInfo> ConsumeRouteConfiguration { get; set; }
		public ISerializationStrategy DefaultSerializationStrategy { get; set; }
		public IDeadLetterStrategy DefaultDeadLetterStrategy { get; set; }
		public IList<AutoSubscription> AutoSubscriptions { get; set; }
		public IQueueStrategy ConnectionDownQueueStrategy { get; set; }
		public TimeSpan ReconnectionInterval { get; set; }
	}
}