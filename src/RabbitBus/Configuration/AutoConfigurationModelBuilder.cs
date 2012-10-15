using System.Collections.Generic;
using System.Reflection;
using RabbitBus.Configuration.Internal;

namespace RabbitBus.Configuration
{
	public class AutoConfigurationModelBuilder
	{
		readonly IList<Assembly> _assemblies = new List<Assembly>();

		readonly IList<IConsumeConfigurationConvention> _consumeConfigurationConventions =
			new List<IConsumeConfigurationConvention>();

		readonly IList<IPublishConfigurationConvention> _publishConfigurationConventions =
			new List<IPublishConfigurationConvention>();

		readonly IList<ISubscriptionConvention> _subscriptionConventions = new List<ISubscriptionConvention>();
		IDependencyResolver _dependencyResolver = new DefaultDependencyResolver();

		public IAutoConfigurationModel Build()
		{
			return new AutoConfigurationModel
			       	{
			       		Assemblies = _assemblies,
			       		ConsumeConfigurationConventions = _consumeConfigurationConventions,
			       		PublishConfigurationConventions = _publishConfigurationConventions,
			       		SubscriptionConventions = _subscriptionConventions,
			       		DependencyResolver = _dependencyResolver
			       	};
		}

		public AutoConfigurationModelBuilder WithAssembly(Assembly assembly)
		{
			_assemblies.Add(assembly);
			return this;
		}

		public AutoConfigurationModelBuilder WithCallingAssembly()
		{
			_assemblies.Add(Assembly.GetCallingAssembly());
			return this;
		}

		public AutoConfigurationModelBuilder WithPublishConfigurationConvention(
			IPublishConfigurationConvention publishConfigurationConvention)
		{
			_publishConfigurationConventions.Add(publishConfigurationConvention);
			return this;
		}

		public AutoConfigurationModelBuilder WithConsumeConfigurationConvention(
			IConsumeConfigurationConvention consumeConfigurationConvention)
		{
			_consumeConfigurationConventions.Add(consumeConfigurationConvention);
			return this;
		}

		public AutoConfigurationModelBuilder WithSubscriptionConvention(ISubscriptionConvention subscriptionConvention)
		{
			_subscriptionConventions.Add(subscriptionConvention);
			return this;
		}

		public AutoConfigurationModelBuilder WithDefaultConventions()
		{
			_subscriptionConventions.Add(new MessageHandlerSubscrptionConvention());
			return this;
		}

		public AutoConfigurationModelBuilder WithDependencyResolver(IDependencyResolver dependencyResolver)
		{
			_dependencyResolver = dependencyResolver;
			return this;
		}
	}
}