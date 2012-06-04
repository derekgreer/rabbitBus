using System;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace RabbitBus
{
	public interface IMessageContext<out TMessage>
	{
		ulong Id { get; }
		TMessage Message { get; }
		bool AcceptanceRequired { get; }
		DateTime TimeStamp { get; }
		bool Redelivered { get; }
		string CorrelationId { get; }
		string UserId { get; }
		void AcceptMessage();
		void RejectMessage(bool requeue);
		void Reply<TResponseMessage>(TResponseMessage responseMessage);
	}

	public class MessageContext<TMessage> : IMessageContext<TMessage>
	{
		readonly IBasicProperties _basicProperties;
		readonly byte[] _body;
		readonly IModel _channel;
		readonly IConsumeInfo _consumeInfo;
		readonly IDeadLetterStrategy _deadLetterStrategy;
		readonly IMessagePublisher _messagePublisher;

		public MessageContext(IDeadLetterStrategy deadLetterStrategy, TMessage message, IConsumeInfo consumeInfo,
		                      IModel channel, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
		                      IBasicProperties basicProperties, byte[] body, IMessagePublisher messagePublisher)
		{
			_deadLetterStrategy = deadLetterStrategy;
			_consumeInfo = consumeInfo;
			_channel = channel;
			Id = deliveryTag;
			Redelivered = redelivered;
			Exchange = exchange;
			RoutingKey = routingKey;
			_basicProperties = basicProperties;
			_body = body;
			_messagePublisher = messagePublisher;
			Message = message;
		}

		public string RoutingKey { get; private set; }

		public string Exchange { get; private set; }

		public bool Redelivered { get; private set; }

		public string CorrelationId
		{
			get { return _basicProperties.CorrelationId; }
		}

		public string UserId
		{
			get { return _basicProperties.UserId; }
		}

		public ulong Id { get; private set; }

		public TMessage Message { get; private set; }

		public void AcceptMessage()
		{
			_channel.BasicAck(Id, false);
		}

		public void RejectMessage(bool requeue)
		{
			if (!requeue)
			{
				_deadLetterStrategy.Publish(_basicProperties, _body);
			}
			_channel.BasicNack(Id, false, requeue);
		}

		public void Reply<TReplyMessage>(TReplyMessage responseMessage)
		{
			if (_basicProperties.ReplyTo != null)
			{
				PublicationAddress publicationAddress = PublicationAddress.Parse(_basicProperties.ReplyTo);
				var replyProperties = new BasicProperties();
				replyProperties.CorrelationId = _basicProperties.CorrelationId;
				_messagePublisher.PublishReply<TMessage, TReplyMessage>(publicationAddress, responseMessage, replyProperties);
			}
		}

		public bool AcceptanceRequired
		{
			get { return !_consumeInfo.IsAutoAcknowledge; }
		}

		public DateTime TimeStamp
		{
			get { return _basicProperties.Timestamp.ToDateTime(); }
		}
	}

	public static class AmqpTimestampExtensions
	{
		static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public static DateTime ToDateTime(this AmqpTimestamp timestamp)
		{
			return UnixEpoch.AddSeconds(timestamp.UnixTime);
		}
	}
}