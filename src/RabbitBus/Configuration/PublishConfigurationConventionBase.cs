using System;
using RabbitMQ.Client;

namespace RabbitBus.Configuration
{
	public abstract class PublishConfigurationConventionBase : IPublishConfigurationConvention
	{
		public abstract bool ShouldRegister(Type type);

		public abstract string GetExchangeName(Type type);
		
		public string GetExchangeType(Type type)
		{
			return ExchangeType.Direct;
		}

		public bool IsAutoAcknowledge(Type type)
		{
			return false;
		}

		public string GetDefaultRouteKey(Type type)
		{
			return string.Empty;
		}

		public bool IsExclusive(Type type)
		{
			return false;
		}

		public bool IsAutoDeleteExchange(Type type)
		{
			return true;
		}

		public bool IsDurableExchange(Type type)
		{
			return false;
		}

		public bool IsPersistent(Type type)
		{
			return false;
		}

		public ISerializationStrategy GetSerializationStrategy(Type type)
		{
			return null;
		}

		public bool IsSigned(Type type)
		{
			return false;
		}
	}
}