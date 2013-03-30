using System;
using System.Collections;
using RabbitMQ.Client;

namespace RabbitBus
{
	public interface IMessagePublisher
	{
		void Publish(object message, MessageProperties messageProperties);
		void Publish<TRequestMessage, TReplyMessage>(TRequestMessage message, MessageProperties messageProperties, Action<IMessageContext<TReplyMessage>> replyAction, TimeSpan timeout);
		void Flush();
		void SetConnection(IConnection connection);
		void PublishReply<TRequestMessage, TReplyMessage>(PublicationAddress publicationAddress, TReplyMessage message, IBasicProperties responseMessage);
		
	}
}