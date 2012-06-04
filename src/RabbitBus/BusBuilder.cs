using System;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;

namespace RabbitBus
{
	public class BusBuilder
	{
		readonly ConfigurationContext _configurationContext;

		public BusBuilder()
		{
			_configurationContext = new ConfigurationContext();
		}
		
		public Bus Build()
		{
			return new Bus(_configurationContext.ConfigurationModel);
		}

		public BusBuilder Configure(Action<IConfigurationContext> action)
		{
			action(_configurationContext);
			return this;
		}
	}
}