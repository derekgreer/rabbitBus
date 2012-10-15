using System;
using System.Linq;

namespace RabbitBus.Configuration
{
	public class MessageHandlerSubscrptionConvention : ISubscriptionConvention
	{
		public bool ShouldRegister(Type handlerType)
		{
			return
				handlerType.GetInterfaces()
					.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IMessageHandler<>));
		}

		public Action<IMessageContext<TMessage>> GetMessageHandler<TMessage>(object handler)
		{
			return ((IMessageHandler<TMessage>) handler).Handle;
		}

		public Type GetMessageType(object handler)
		{
			return handler.GetType().GetInterfaces()
				.FirstOrDefault(i => i.IsGenericType &&
				                     i.GetGenericTypeDefinition() == typeof (IMessageHandler<>))
				.GetGenericArguments()[0];
		}
	}
}