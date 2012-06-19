using System;

namespace RabbitBus.Configuration
{
	public interface IPublishConfigurationConvention
	{
		bool ShouldRegister(Type type);
		string GetExchangeName(Type type);
		string GetExchangeType(Type type);
		bool IsAutoAcknowledge(Type type);
		string GetDefaultRouteKey(Type type);
		bool IsExclusive(Type type);
		bool IsAutoDeleteExchange(Type type);
		bool IsDurableExchange(Type type);
		bool IsPersistent(Type type);
		ISerializationStrategy GetSerializationStrategy(Type type);
		bool IsSigned(Type type);
	}
}