using System.Collections.Generic;
using System.Reflection;
using RabbitBus.Configuration.Internal;

namespace RabbitBus.Configuration
{
	public class AutoSubscriptionModelBuilder
	{
		readonly IList<Assembly> _assemblies = new List<Assembly>();
		readonly IList<ISubscriptionConvention> _subscriptionConventions = new List<ISubscriptionConvention>();
		readonly IList<IHandlerConvention> _handlerConventions = new List<IHandlerConvention>();

		public AutoSubscriptionModelBuilder WithAssembly(Assembly assembly)
		{
			_assemblies.Add(assembly);
			return this;
		}

		public AutoSubscriptionModelBuilder WithSubscriptionConvention(ISubscriptionConvention subscriptionConvention)
		{
			_subscriptionConventions.Add(subscriptionConvention);
			return this;
		}

		public IAutoSubscriptionModel Build()
		{
			return new AutoSubscriptionModel
			       	{
			       		Assemblies = _assemblies,
			       		Conventions = _subscriptionConventions,
			       		HandlerConventions = _handlerConventions
			       	};
		}

		public AutoSubscriptionModelBuilder WithHandlerConvention(IHandlerConvention handlerConvention)
		{
			_handlerConventions.Add(handlerConvention);
			return this;
		}
	}
}