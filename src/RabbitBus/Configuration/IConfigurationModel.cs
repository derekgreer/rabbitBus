using System;
using System.Collections.Generic;

namespace RabbitBus.Configuration
{
	public interface IConfigurationModel
	{
		IRouteConfiguration<IPublishInfo> PublicationRouteConfiguration { get; set; }
		IRouteConfiguration<IConsumeInfo> ConsumeRouteConfiguration { get; set; }
		ISerializationStrategy DefaultSerializationStrategy { get; set; }
		IDeadLetterStrategy DefaultDeadLetterStrategy { get; set; }
		IList<AutoSubscription> AutoSubscriptions { get; set; }
		IQueueStrategy ConnectionDownQueueStrategy { get; set; }
		TimeSpan ReconnectionInterval { get; set; }
	}
}