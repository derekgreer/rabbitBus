using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
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
		readonly ISerializationStrategy _defaultSerializationStrategy;
		readonly IRouteConfiguration<IPublishInfo> _publishRouteConfiguration;
		readonly object _queueLock = new object();
		readonly IQueueStrategy _queueStrategy;
		readonly string _userName;
		IConnection _connection;

		public MessagePublisher(string userName,
		                        IRouteConfiguration<IPublishInfo> publishRouteConfiguration,
		                        IRouteConfiguration<IConsumeInfo> consumeRouteConfiguration,
		                        ISerializationStrategy defaultSerializationStrategy,
		                        IQueueStrategy queueStrategy)
		{
			_userName = userName;
			_publishRouteConfiguration = publishRouteConfiguration;
			_consumeRouteConfiguration = consumeRouteConfiguration;
			_defaultSerializationStrategy = defaultSerializationStrategy;
			_queueStrategy = queueStrategy;
		}

		public void Publish(object message, string routingKey, IDictionary headers)
		{
			try
			{
				if (_connection != null && _connection.IsOpen)
				{
					Flush();
					PublishMessage(message, routingKey, headers);
				}
				else
				{
					lock (_queueLock)
						_queueStrategy.Enqueue(new MessageInfo {Message = message, RoutingKey = routingKey, Headers = headers});
				}
			}
			catch (Exception e)
			{
				Logger.Current.Write("An exception occurred while publishing: " + e.Message, TraceEventType.Error);
				throw;
			}
		}

		public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey, IDictionary headers,
		                                                    Action<IMessageContext<TReplyMessage>> replyAction)
		{
			try
			{
				PublishMessage(message, routingKey, headers, replyAction);
			}
			catch (Exception e)
			{
				Logger.Current.Write("An exception occurred while publishing: " + e.Message, TraceEventType.Error);
			}
		}

		public void Flush()
		{
			while (_queueStrategy.Count != 0)
			{
				MessageInfo messageInfo = _queueStrategy.Dequeue();
				PublishMessage(messageInfo.Message, messageInfo.RoutingKey, messageInfo.Headers);
			}
		}

		public void SetConnection(IConnection connection)
		{
			_connection = connection;
		}

		public void PublishReply<TRequestMessage, TReplyMessage>(PublicationAddress publicationAddress,
		                                                         TReplyMessage replyMessage,
		                                                         IBasicProperties replyProperties)
		{
			IConsumeInfo consumeInfo = _consumeRouteConfiguration.GetRouteInfo(typeof (TRequestMessage));
			IModel channel = _connection.CreateModel();
			if(publicationAddress.ExchangeName != string.Empty)
			{
				channel.ExchangeDeclare(publicationAddress.ExchangeName, publicationAddress.ExchangeType,
				                        false, true, null);
			}
			ISerializationStrategy serializationStrategy = consumeInfo.SerializationStrategy ??
			                                               _defaultSerializationStrategy;
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

		void PublishMessage(object message, string routingKey, IDictionary headers)
		{
			IPublishInfo publishInfo = _publishRouteConfiguration.GetRouteInfo(message.GetType());
			IModel channel = _connection.CreateModel();
			channel.ExchangeDeclare(publishInfo.ExchangeName, publishInfo.ExchangeType,
			                        publishInfo.IsDurable,
			                        publishInfo.IsAutoDelete, null);
			ISerializationStrategy serializationStrategy = publishInfo.SerializationStrategy ??
			                                               _defaultSerializationStrategy;
			byte[] bytes = serializationStrategy.Serialize(message);

			var properties = new BasicProperties();

			ListDictionary messageHeaders = GetHeaders(headers, publishInfo.DefaultHeaders);

			if (messageHeaders.Count != 0)
			{
				properties.Headers = messageHeaders;
			}

			properties.SetPersistent(publishInfo.IsPersistent);
			properties.ContentType = serializationStrategy.ContentType;
			properties.ContentEncoding = serializationStrategy.ContentEncoding;
			if (publishInfo.IsSigned)
				properties.UserId = _userName;
			properties.CorrelationId = Guid.NewGuid().ToString();
			channel.BasicPublish(publishInfo.ExchangeName, routingKey ?? publishInfo.DefaultRoutingKey, properties, bytes);
			channel.Close();

			string log = string.Format("Published message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
			                           _connection.Endpoint.HostName,
			                           _connection.Endpoint.Port,
			                           publishInfo.ExchangeName,
			                           routingKey);

			Logger.Current.Write(log, TraceEventType.Information);
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

		void PublishMessage<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey, IDictionary headers,
		                                                    Action<IMessageContext<TReplyMessage>> replyAction)
		{
			IPublishInfo publishInfo = _publishRouteConfiguration.GetRouteInfo(typeof (TRequestMessage));
			IModel channel = _connection.CreateModel();
			channel.ExchangeDeclare(publishInfo.ExchangeName, publishInfo.ExchangeType,
			                        publishInfo.IsDurable,
			                        publishInfo.IsAutoDelete, null);

			ISerializationStrategy serializationStrategy = publishInfo.SerializationStrategy ??
			                                               _defaultSerializationStrategy;
			byte[] bytes = serializationStrategy.Serialize(message);

			var properties = new BasicProperties();
			ListDictionary messageHeaders = GetHeaders(headers, publishInfo.DefaultHeaders);

			if (messageHeaders.Count != 0)
			{
				properties.Headers = messageHeaders;
			}
			properties.SetPersistent(publishInfo.IsPersistent);
			properties.ContentType = serializationStrategy.ContentType;
			properties.ContentEncoding = serializationStrategy.ContentEncoding;
			if (publishInfo.IsSigned)
				properties.UserId = _userName;

			var replyInfo = publishInfo.ReplyInfo;
			string queueName = Guid.NewGuid().ToString();
			
			var replyInfoCopy = new ConsumeInfo
			                  	{
			                  		DefaultRoutingKey = replyInfo.DefaultRoutingKey,
			                  		ErrorCallback = replyInfo.ErrorCallback,
			                  		ExchangeName = "",
			                  		ExchangeType = replyInfo.ExchangeType,
			                  		Exclusive = replyInfo.Exclusive,
			                  		IsAutoAcknowledge = replyInfo.IsAutoAcknowledge,
			                  		IsExchangeAutoDelete = replyInfo.IsExchangeAutoDelete,
			                  		IsExchangeDurable = replyInfo.IsExchangeDurable,
			                  		IsQueueAutoDelete = replyInfo.IsQueueAutoDelete,
			                  		IsQueueDurable = replyInfo.IsQueueDurable,
			                  		QualityOfService = replyInfo.QualityOfService,
			                  		QueueName = queueName,
			                  		SerializationStrategy = replyInfo.SerializationStrategy
			                  	};
			

			properties.ReplyTo =
				new PublicationAddress(replyInfo.ExchangeType, "", queueName).ToString();
			properties.CorrelationId = Guid.NewGuid().ToString();

			new Task(() => new Subscription<TReplyMessage>(_connection,
			                                               new DefaultDeadLetterStrategy(),
			                                               serializationStrategy,
			                                               replyInfoCopy,
			                                               queueName /* routing key */,
			                                               replyAction,
			                                               null,
			                                               x => { },
			                                               this, SubscriptionType.RemoteProcedure
			               	).Start()).Start();


			channel.BasicPublish(publishInfo.ExchangeName, routingKey ?? publishInfo.DefaultRoutingKey, properties, bytes);
			channel.Close();

			string log = string.Format("Published message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
			                           _connection.Endpoint.HostName,
			                           _connection.Endpoint.Port,
			                           publishInfo.ExchangeName,
			                           routingKey);

			Logger.Current.Write(log, TraceEventType.Information);
		}
	}
}