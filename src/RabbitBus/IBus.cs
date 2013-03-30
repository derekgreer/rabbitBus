using System;

namespace RabbitBus
{
	public interface IBus
	{
		void Publish<TMessage>(TMessage message, MessageProperties messageProperties);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, MessageProperties messageProperties, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action, MessageProperties messageProperties);
		void Unsubscribe<TMessage>(MessageProperties messageProperties);
		IConsumerContext<TMessage> CreateConsumerContext<TMessage>();
	}
}