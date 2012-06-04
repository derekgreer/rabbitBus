using System;
using RabbitMQ.Client;

namespace RabbitBus.Configuration
{
	public abstract class SubscriptionConventionBase : ISubscriptionConvention
	{
		public abstract bool ShouldRegister(Type type);

		public abstract string GetExchangeName(Type type);

		public abstract string GetQueueName(Type type);
	
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

		public bool IsAutoDeleteQueue(Type type)
		{
			return true;
		}

		public bool IsDurableQueue(Type type)
		{
			return false;
		}

		public ISerializationStrategy GetSerializationStrategy(Type type)
		{
			return null;
		}

		public Action<IErrorContext> GetErrorCallback(Type type)
		{
			return null;
		}
	}
}