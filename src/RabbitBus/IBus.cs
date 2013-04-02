using System;
using System.Collections;

namespace RabbitBus
{
	public interface IBus
	{
		void Publish<TMessage>(TMessage message);
		void Publish<TMessage>(TMessage message, string routingKey);
		void Publish<TMessage>(TMessage message, IDictionary headers);
		void Publish<TMessage>(TMessage message, int? expiration);
		void Publish<TMessage>(TMessage message, string routingKey, int? expiration);
		void Publish<TMessage>(TMessage message, IDictionary headers, int? expiration);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, int? expiration, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, int? expiration, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, int? expiration, Action<IMessageContext<TReplyMessage>> action);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, int? expiration, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, int? expiration, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, int? expiration, Action<IMessageContext<TReplyMessage>> action, TimeSpan callbackTimeout);
		void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action);
		void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action, string routingKey);
		void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action, IDictionary headers);
		void Unsubscribe<TMessage>();
		void Unsubscribe<TMessage>(string routingKey);
		void Unsubscribe<TMessage>(IDictionary headers);
		IConsumerContext<TMessage> CreateConsumerContext<TMessage>();
	}
}
