using System.Collections.Generic;
using System.Reflection;

namespace RabbitBus.Configuration
{
	public interface IAutoConfigurationModel
	{
		IEnumerable<Assembly> Assemblies { get; set; }
		IEnumerable<IConsumeConfigurationConvention> ConsumeConfigurationConventions { get; set; }
		IEnumerable<IPublishConfigurationConvention> PublishConfigurationConventions { get; set; }
		IEnumerable<ISubscriptionConvention> SubscriptionConventions { get; set; }
	}
}