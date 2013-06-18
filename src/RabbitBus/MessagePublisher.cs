using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace RabbitBus
{
	class MessagePublisher : IMessagePublisher
	{
		readonly IRouteConfiguration<IConsumeInfo> _consumeRouteConfiguration;
		readonly IDeadLetterConfiguration _defaultDeadLetterConfiguration;
		readonly ISerializationStrategy _defaultSerializationStrategy;
		readonly IRouteConfiguration<IPublishInfo> _publishRouteConfiguration;
		readonly object _queueLock = new object();
		readonly IQueueStrategy _queueStrategy;
		readonly string _userName;
		IConnection _connection;

		public MessagePublisher(string userName,
								IDeadLetterConfiguration defaultDeadLetterConfiguration,
								IRouteConfiguration<IPublishInfo> publishRouteConfiguration,
								IRouteConfiguration<IConsumeInfo> consumeRouteConfiguration,
								ISerializationStrategy defaultSerializationStrategy,
								IQueueStrategy queueStrategy)
		{
			_userName = userName;
			_defaultDeadLetterConfiguration = defaultDeadLetterConfiguration;

			_publishRouteConfiguration = publishRouteConfiguration;
			_consumeRouteConfiguration = consumeRouteConfiguration;
			_defaultSerializationStrategy = defaultSerializationStrategy;
			_queueStrategy = queueStrategy;
		}

		public void Flush()
		{
			while (_queueStrategy.Count != 0)
			{
				MessageInfo messageInfo = _queueStrategy.Dequeue();
				PublishMessage(messageInfo.Message, messageInfo.MessageProperties, null);
			}
		}

		public void SetConnection(IConnection connection)
		{
			_connection = connection;
		}

		public void Publish(object message, MessageProperties messageProperties)
		{
			try
			{
				if (_connection != null && _connection.IsOpen)
				{
					Flush();
					PublishMessage(message, messageProperties, null);
				}
				else
				{
					lock (_queueLock)
						_queueStrategy.Enqueue(new MessageInfo {Message = message, MessageProperties = messageProperties});
				}
			}
			catch (Exception e)
			{
				Logger.Current.Write("An exception occurred while publishing: " + e.Message, TraceEventType.Error);
				throw;
			}
		}

		public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage message, MessageProperties messageProperties,
															Action<IMessageContext<TReplyMessage>> replyAction,
															TimeSpan timeout)
		{
			try
			{
				PublishMessage(message, messageProperties, replyAction, timeout);
			}
			catch (Exception e)
			{
				Logger.Current.Write("An exception occurred while publishing: " + e.Message, TraceEventType.Error);
			}
		}


		public void PublishReply<TRequestMessage, TReplyMessage>(PublicationAddress publicationAddress,
																 TReplyMessage replyMessage,
																 IBasicProperties replyProperties)
		{
			IModel channel = _connection.CreateModel();
			if (publicationAddress.ExchangeName != string.Empty)
			{
				channel.ExchangeDeclare(publicationAddress.ExchangeName, publicationAddress.ExchangeType, false, true, null);
			}
			IConsumeInfo consumeInfo = _consumeRouteConfiguration.GetRouteInfo(typeof (TRequestMessage));
			ISerializationStrategy serializationStrategy = consumeInfo.SerializationStrategy ?? _defaultSerializationStrategy;
			byte[] bytes = serializationStrategy.Serialize(replyMessage);
			channel.BasicPublish(publicationAddress, replyProperties, bytes);
			channel.Close();

			string log = string.Format("Published reply message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
									   _connection.Endpoint.HostName,
									   _connection.Endpoint.Port,
									   publicationAddress.ExchangeName,
									   publicationAddress.RoutingKey);

			Logger.Current.Write(log, TraceEventType.Information);
		}

		void PublishMessage(object message, MessageProperties messageProperties,
							Action<IBasicProperties, IPublishInfo> replyAction)
		{
			IPublishInfo publishInfo = _publishRouteConfiguration.GetRouteInfo(message.GetType());
			IModel channel = _connection.CreateModel();
			channel.ExchangeDeclare(publishInfo.ExchangeName, publishInfo.ExchangeType,
									publishInfo.IsDurable,
									publishInfo.IsAutoDelete, null);
			ISerializationStrategy serializationStrategy = publishInfo.SerializationStrategy ?? _defaultSerializationStrategy;
			byte[] bytes = serializationStrategy.Serialize(message);

			var properties = new BasicProperties();

			ListDictionary messageHeaders = GetHeaders(messageProperties.Headers, publishInfo.DefaultHeaders);

			if (messageHeaders.Count != 0)
			{
				properties.Headers = messageHeaders;
			}

			properties.SetPersistent(publishInfo.IsPersistent);
			properties.ContentType = serializationStrategy.ContentType;
			properties.ContentEncoding = serializationStrategy.ContentEncoding;
			if (publishInfo.IsSigned)
				properties.UserId = _userName;
			properties.CorrelationId = messageProperties.CorrelationId ?? Guid.NewGuid().ToString();

			if (messageProperties.Expiration.HasValue)
			{
				properties.Expiration = messageProperties.Expiration.Value.TotalMilliseconds.ToString();
			}
			else if (publishInfo.Expiration.HasValue)
			{
				properties.Expiration = publishInfo.Expiration.Value.TotalMilliseconds.ToString();
			}

			if (replyAction != null)
			{
				replyAction(properties, publishInfo);
			}

			channel.BasicPublish(publishInfo.ExchangeName, messageProperties.RoutingKey ?? publishInfo.DefaultRoutingKey,
								 properties, bytes);
			channel.Close();

			string log = string.Format("Published message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
									   _connection.Endpoint.HostName,
									   _connection.Endpoint.Port,
									   publishInfo.ExchangeName,
									   messageProperties.RoutingKey ?? publishInfo.DefaultRoutingKey);

			Logger.Current.Write(log, TraceEventType.Information);
		}

		void PublishMessage<TRequestMessage, TReplyMessage>(TRequestMessage message, MessageProperties messageProperties,
															Action<IMessageContext<TReplyMessage>> replyAction,
															TimeSpan timeout)
		{
			PublishMessage(message, messageProperties, (p, pi) =>
				{
					IConsumeInfo replyInfo = pi.ReplyInfo;
					string queueName = Guid.NewGuid().ToString();
					p.ReplyTo = new PublicationAddress(replyInfo.ExchangeType, "", queueName).ToString();
					p.CorrelationId = messageProperties.CorrelationId ?? Guid.NewGuid().ToString();
					ISerializationStrategy serializationStrategy = pi.SerializationStrategy ?? _defaultSerializationStrategy;

					IConsumeInfo consumeInfo = CloneConsumeInfo(replyInfo);
					consumeInfo.ExchangeName = "";
					consumeInfo.QueueName = queueName;
					consumeInfo.IsQueueExclusive = true;

					new Subscription<TReplyMessage>(_connection,
													_defaultDeadLetterConfiguration,
													serializationStrategy,
													consumeInfo,
													queueName /* routing key */,
													replyAction,
													null,
													x => { },
													this,
													SubscriptionType.RemoteProcedure,
													timeout).Start();
				});
		}


		static ListDictionary GetHeaders(IDictionary headers, IDictionary defaultHeaders)
		{
			var messageHeaders = new ListDictionary();

			if (defaultHeaders != null)
			{
				foreach (object key in defaultHeaders.Keys)
				{
					messageHeaders.Add(key, defaultHeaders[key]);
				}
			}

			if (headers != null)
			{
				foreach (object key in headers.Keys)
				{
					messageHeaders.Add(key, headers[key]);
				}
			}
			return messageHeaders;
		}

		IConsumeInfo CloneConsumeInfo(IConsumeInfo consumeInfo)
		{
			return new ConsumeInfo
				{
					ExchangeName = consumeInfo.ExchangeName,
					QueueName = consumeInfo.QueueName,
					DefaultRoutingKey = consumeInfo.DefaultRoutingKey,
					IsQueueExclusive = consumeInfo.IsQueueExclusive,
					IsAutoAcknowledge = consumeInfo.IsAutoAcknowledge,
					IsQueueAutoDelete = consumeInfo.IsQueueAutoDelete,
					IsExchangeAutoDelete = consumeInfo.IsExchangeAutoDelete,
					IsQueueDurable = consumeInfo.IsQueueDurable,
					IsExchangeDurable = consumeInfo.IsExchangeDurable,
					ExchangeType = consumeInfo.ExchangeType,
					SerializationStrategy = consumeInfo.SerializationStrategy,
					ErrorCallback = consumeInfo.ErrorCallback,
					QualityOfService = consumeInfo.QualityOfService
				};
		}
	}
}