using System;

namespace RabbitBus.Configuration
{
	public interface ISubscriptionConvention
	{
		bool ShouldRegister(Type type);
		string GetExchangeName(Type type);
		string GetQueueName(Type type);
		string GetExchangeType(Type type);
		bool IsAutoAcknowledge(Type type);
		string GetDefaultRouteKey(Type type);
		bool IsExclusive(Type type);
		bool IsAutoDeleteExchange(Type type);
		bool IsDurableExchange(Type type);
		bool IsAutoDeleteQueue(Type type);
		bool IsDurableQueue(Type type);
		ISerializationStrategy GetSerializationStrategy(Type type);
		Action<IErrorContext> GetErrorCallback(Type type);
	}
}