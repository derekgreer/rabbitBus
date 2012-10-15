using System.Collections.Generic;
using System.Reflection;

namespace RabbitBus.Configuration.Internal
{
	class AutoConfigurationModel : IAutoConfigurationModel
	{
		public IEnumerable<Assembly> Assemblies { get; set; }
		public IEnumerable<IConsumeConfigurationConvention> ConsumeConfigurationConventions { get; set; }
		public IEnumerable<IPublishConfigurationConvention> PublishConfigurationConventions { get; set; }
		public IEnumerable<ISubscriptionConvention> SubscriptionConventions { get; set; }
		public IDependencyResolver DependencyResolver { get; set; }
	}
}