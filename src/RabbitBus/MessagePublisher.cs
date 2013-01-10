#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9_1;

#endregion

namespace RabbitBus
{
    internal class MessagePublisher : IMessagePublisher
    {
        private readonly List<ISubscription> _callbackSubscriptions;
        private readonly object _callbacksLock = new object();
        private readonly IRouteConfiguration<IConsumeInfo> _consumeRouteConfiguration;
        private readonly ISerializationStrategy _defaultSerializationStrategy;
        private readonly IRouteConfiguration<IPublishInfo> _publishRouteConfiguration;
        private readonly object _queueLock = new object();
        private readonly IQueueStrategy _queueStrategy;
        private readonly string _userName;
        private IConnection _connection;


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
            _callbackSubscriptions = new List<ISubscription>();
        }

        public void Flush()
        {
            while (_queueStrategy.Count != 0)
            {
                var messageInfo = _queueStrategy.Dequeue();
                PublishMessage(messageInfo.Message, messageInfo.RoutingKey, messageInfo.Headers, null);
            }
        }

        public void SetConnection(IConnection connection)
        {
            _connection = connection;
        }

        public void Publish(object message, string routingKey, IDictionary headers)
        {
            try
            {
                if (_connection != null && _connection.IsOpen)
                {
                    Flush();
                    PublishMessage(message, routingKey, headers, null);
                }
                else
                {
                    lock (_queueLock)
                        _queueStrategy.Enqueue(new MessageInfo
                            {
                                Message = message,
                                RoutingKey = routingKey,
                                Headers = headers
                            });
                }
            }
            catch (Exception e)
            {
                Logger.Current.Write("An exception occurred while publishing: " + e.Message, TraceEventType.Error);
                throw;
            }
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey,
                                                            IDictionary headers,
                                                            Action<IMessageContext<TReplyMessage>> replyAction,
                                                            TimeSpan timeout)
        {
            try
            {
                PublishMessage(message, routingKey, headers, replyAction, timeout);
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
            var channel = _connection.CreateModel();
            channel.CallbackException += ChannelCallbackException;
            if (publicationAddress.ExchangeName != string.Empty)
            {
                channel.ExchangeDeclare(publicationAddress.ExchangeName, publicationAddress.ExchangeType, false, true,
                                        null);
            }
            var consumeInfo = _consumeRouteConfiguration.GetRouteInfo(typeof(TRequestMessage));
            var serializationStrategy = consumeInfo.SerializationStrategy ?? _defaultSerializationStrategy;
            var bytes = serializationStrategy.Serialize(replyMessage);
            channel.BasicPublish(publicationAddress, replyProperties, bytes);
            channel.Close();

            var log = string.Format("Published reply message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
                                    _connection.Endpoint.HostName,
                                    _connection.Endpoint.Port,
                                    publicationAddress.ExchangeName,
                                    publicationAddress.RoutingKey);

            Logger.Current.Write(log, TraceEventType.Information);
        }

        private void PublishMessage(object message, string routingKey, IDictionary headers,
                                    Action<IBasicProperties, IPublishInfo> replyAction)
        {
            var publishInfo = _publishRouteConfiguration.GetRouteInfo(message.GetType());
            var channel = _connection.CreateModel();
            channel.CallbackException += ChannelCallbackException;
            if (publishInfo.ExchangeName != string.Empty)
            {
                // only declare if not default exchange
                channel.ExchangeDeclare(publishInfo.ExchangeName, publishInfo.ExchangeType,
                                        publishInfo.IsDurable,
                                        publishInfo.IsAutoDelete, null);
            }
            var serializationStrategy = publishInfo.SerializationStrategy ?? _defaultSerializationStrategy;
            var bytes = serializationStrategy.Serialize(message);

            var properties = new BasicProperties();

            var messageHeaders = GetHeaders(headers, publishInfo.DefaultHeaders);

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

            if (replyAction != null)
                replyAction(properties, publishInfo);

            channel.BasicPublish(publishInfo.ExchangeName, routingKey ?? publishInfo.DefaultRoutingKey, properties,
                                 bytes);
            channel.Close();

            var log = string.Format("Published message to host: {0}, port: {1}, exchange: {2}, routingKey: {3}",
                                    _connection.Endpoint.HostName,
                                    _connection.Endpoint.Port,
                                    publishInfo.ExchangeName,
                                    routingKey);

            Logger.Current.Write(log, TraceEventType.Information);
        }

        void ChannelCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            OnException(e);
        }

        private void PublishMessage<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey,
                                                                    IDictionary headers,
                                                                    Action<IMessageContext<TReplyMessage>> replyAction,
                                                                    TimeSpan timeout)
        {
            PublishMessage(message, routingKey, headers, (p, pi) =>
                {
                    var replyInfo = pi.ReplyInfo;
                    var queueName = Guid.NewGuid().ToString();
                    p.ReplyTo = new PublicationAddress(replyInfo.ExchangeType, "", queueName).ToString();
                    p.CorrelationId = Guid.NewGuid().ToString();
                    var serializationStrategy = pi.SerializationStrategy ?? _defaultSerializationStrategy;

                    var consumeInfo = CloneConsumeInfo(replyInfo);
                    consumeInfo.ExchangeName = "";
                    consumeInfo.QueueName = queueName;
                    consumeInfo.Exclusive = true;

                    var sub = new Subscription<TReplyMessage>(_connection,
                                                              new DefaultDeadLetterStrategy(),
                                                              serializationStrategy,
                                                              consumeInfo,
                                                              queueName /* routing key */,
                                                              replyAction,
                                                              null,
                                                              x => { },
                                                              this,
                                                              SubscriptionType.RemoteProcedure,
                                                              timeout);

                    sub.Stopped += CallbackSubscriptionStopped;

                    lock (_callbacksLock)
                    {
                        _callbackSubscriptions.Add(sub);
                    }

                    sub.Start();
                });
        }

        private void CallbackSubscriptionStopped(object sender, EventArgs e)
        {
            lock (_callbacksLock)
            {
                _callbackSubscriptions.Remove((ISubscription)sender);
            }
        }

        private static ListDictionary GetHeaders(IDictionary headers, IDictionary defaultHeaders)
        {
            var messageHeaders = new ListDictionary();

            if (defaultHeaders != null)
            {
                foreach (var key in defaultHeaders.Keys)
                {
                    messageHeaders.Add(key, defaultHeaders[key]);
                }
            }

            if (headers != null)
            {
                foreach (var key in headers.Keys)
                {
                    messageHeaders.Add(key, headers[key]);
                }
            }
            return messageHeaders;
        }

        private IConsumeInfo CloneConsumeInfo(IConsumeInfo consumeInfo)
        {
            return new ConsumeInfo
                {
                    ExchangeName = consumeInfo.ExchangeName,
                    QueueName = consumeInfo.QueueName,
                    DefaultRoutingKey = consumeInfo.DefaultRoutingKey,
                    Exclusive = consumeInfo.Exclusive,
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

        public event CallbackExceptionEventHandler Exception;

        protected void OnException(CallbackExceptionEventArgs e)
        {
            CallbackExceptionEventHandler handler = Exception;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}