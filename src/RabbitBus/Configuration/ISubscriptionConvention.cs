using System;

namespace RabbitBus.Configuration
{
	public interface ISubscriptionConvention
	{
		bool ShouldRegister(Type handlerType);
		Action<IMessageContext<TMessage>> GetMessageHandler<TMessage>(object handler);
		Type GetMessageType(object handler);
	}
}