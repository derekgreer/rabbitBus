using System;
using System.Collections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus
{
	public interface IMessagePublisher
	{
		void Publish(object message, string routingKey, IDictionary headers);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey, IDictionary headers, Action<IMessageContext<TReplyMessage>> replyAction, TimeSpan timeout);
		void Flush();
		void SetConnection(IConnection connection);
		void PublishReply<TRequestMessage, TReplyMessage>(PublicationAddress publicationAddress, TReplyMessage message, IBasicProperties responseMessage);        
	    event CallbackExceptionEventHandler Exception;
	}
}