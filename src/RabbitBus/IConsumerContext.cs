using System;

namespace RabbitBus
{
	public interface IConsumerContext<out TMessage> : IDisposable
	{
		IMessageContext<TMessage> GetMessage();
	}
}