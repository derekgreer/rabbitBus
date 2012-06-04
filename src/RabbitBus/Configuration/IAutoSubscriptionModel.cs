using System.Collections.Generic;
using System.Reflection;

namespace RabbitBus.Configuration
{
	public interface IAutoSubscriptionModel
	{
		IEnumerable<Assembly> Assemblies { get; set; }
		IEnumerable<ISubscriptionConvention> Conventions { get; set; }
		IEnumerable<IHandlerConvention> HandlerConventions { get; set; }
	}
}