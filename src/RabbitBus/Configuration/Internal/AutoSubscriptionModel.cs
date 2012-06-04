using System.Collections.Generic;
using System.Reflection;

namespace RabbitBus.Configuration.Internal
{
	class AutoSubscriptionModel : IAutoSubscriptionModel
	{
		public IEnumerable<Assembly> Assemblies { get; set; }
		public IEnumerable<ISubscriptionConvention> Conventions { get; set; }
		public IEnumerable<IHandlerConvention> HandlerConventions { get; set; }
	}
}