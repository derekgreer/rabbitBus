using System;

namespace RabbitBus.Specs.Infrastructure
{
	public static class RabbitHandlers
	{
		public static Action<IMessageContext<TMessage>> EmptyHandler<TMessage>()
		{
			return messageContext => { };
		}
	}
}