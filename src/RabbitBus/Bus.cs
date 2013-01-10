using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Impl;

namespace RabbitBus
{
    public class Bus : IBus, IDisposable
    {
        readonly IConfigurationModel _configurationModel;
        readonly object _connectionLock = new object();
        readonly Action<IErrorContext> _defaultErrorCallback;
        readonly IDictionary<ISubscriptionKey, ISubscription> _subscriptions;
        bool _closed;
        IConnection _connection;
        ConnectionFactory _connectionFactory;
        bool _disposed;
        IMessagePublisher _messagePublisher;

        public Bus()
            : this(new ConfigurationModel())
        {
        }

        public Bus(IConfigurationModel configurationModel)
        {
            _configurationModel = configurationModel;
            _defaultErrorCallback = OnConsumeError;
            _subscriptions = new Dictionary<ISubscriptionKey, ISubscription>();
        }

        public void Publish<TMessage>(TMessage message)
        {
            PublishMessage(message, null, null);
        }

        public void Publish<TMessage>(TMessage message, IDictionary headers)
        {
            PublishMessage(message, null, headers);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage,
                                                            Action<IMessageContext<TReplyMessage>> action)
        {
            PublishMessage(requestMessage, null, null, action, TimeSpan.MinValue);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, Action<IMessageContext<TReplyMessage>> action)
        {
            PublishMessage(requestMessage, routingKey, null, action, TimeSpan.MinValue);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, Action<IMessageContext<TReplyMessage>> action)
        {
            PublishMessage(requestMessage, null, headers, action, TimeSpan.MinValue);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage,
                                                            Action<IMessageContext<TReplyMessage>> action,
                                                            TimeSpan callbackTimeout)
        {
            PublishMessage(requestMessage, null, null, action, callbackTimeout);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, string routingKey, Action<IMessageContext<TReplyMessage>> action,
                                                            TimeSpan callbackTimeout)
        {
            PublishMessage(requestMessage, routingKey, null, action, callbackTimeout);
        }

        public void Publish<TRequestMessage, TReplyMessage>(TRequestMessage requestMessage, IDictionary headers, Action<IMessageContext<TReplyMessage>> action,
                                                            TimeSpan callbackTimeout)
        {
            PublishMessage(requestMessage, null, headers, action, callbackTimeout);
        }

        public void Publish<TMessage>(TMessage message, string routingKey)
        {
            PublishMessage(message, routingKey, null);
        }

        public void Unsubscribe<TMessage>()
        {
            UnsubscribeMessage<TMessage>(null, null);
        }

        public void Unsubscribe<TMessage>(string routingKey)
        {
            UnsubscribeMessage<TMessage>(routingKey, null);
        }

        public void Unsubscribe<TMessage>(IDictionary headers)
        {
            UnsubscribeMessage<TMessage>(null, headers);
        }

        public void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action)
        {
            SubscribeMessage(action, null, null);
        }

        public void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action, IDictionary headers)
        {
            SubscribeMessage(action, null, headers);
        }

        public void Subscribe<TMessage>(Action<IMessageContext<TMessage>> action, string routingKey)
        {
            SubscribeMessage(action, routingKey, null);
        }

        public IConsumerContext<TMessage> CreateConsumerContext<TMessage>()
        {
            Logger.Current.Write(new LogEntry { Message = "Creating ConsumerContext ...", Severity = TraceEventType.Information });
            return new ConsumerContext<TMessage>(_connection,
                                                 _configurationModel.ConsumeRouteConfiguration.GetRouteInfo(typeof(TMessage)),
                                                 _configurationModel.DefaultSerializationStrategy,
                                                 _configurationModel.DefaultDeadLetterStrategy, _messagePublisher);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void RegisterAutoSubscriptions(IConfigurationModel configurationModel)
        {
            foreach (AutoSubscription autoSubscription in configurationModel.AutoSubscriptions)
            {
                MethodInfo openSubscribeMessage = typeof(Bus).GetMethod("SubscribeMessage",
                                                                         BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo closedSubscribedMessage = openSubscribeMessage.MakeGenericMethod(new[] { autoSubscription.MessageType });
                closedSubscribedMessage.Invoke(this, new[] { autoSubscription.MessageHandler, null, null });
            }
        }

        void UnsubscribeMessage<TMessage>(string routingKey, IDictionary headers)
        {
            ISubscription subscription;
            var key = new SubscriptionKey(typeof(TMessage), routingKey, headers);
            _subscriptions.TryGetValue(key, out subscription);

            if (subscription != null)
            {
                subscription.Stop();
                _subscriptions.Remove(key);
            }
        }

        void PublishMessage<TRequestMessage, TReplyMessage>(TRequestMessage message, string routingKey, IDictionary headers,
                                                            Action<IMessageContext<TReplyMessage>> replyAction,
                                                            TimeSpan timeout)
        {
            _messagePublisher.Publish(message, routingKey, headers, replyAction, timeout);
        }

        void PublishMessage<TMessage>(TMessage message, string routingKey, IDictionary headers)
        {
            _messagePublisher.Publish(message, routingKey, headers);
        }

        public void Connect()
        {
            Connect("amqp://guest:guest@localhost:5672/%2f");
        }

        public void Connect(string amqpUri)
        {
            Connect(amqpUri, TimeSpan.FromSeconds(30));
        }

        public void Connect(string amqpUri, TimeSpan timeout)
        {
            var amqpTcpEndpoint = new AmqpTcpEndpoint(new Uri(amqpUri));

            Logger.Current.Write(string.Format("Establishing connection to host:{0}, port:{1}",
                amqpTcpEndpoint.HostName, amqpTcpEndpoint.Port), TraceEventType.Information);
            _connectionFactory = new ConnectionFactory
                                    {
                                        Uri = amqpUri
                                    };

            _messagePublisher = new MessagePublisher(_connectionFactory.UserName,
                                                     _configurationModel.PublishRouteConfiguration,
                                                     _configurationModel.ConsumeRouteConfiguration,
                                                     _configurationModel.DefaultSerializationStrategy,
                                                     _configurationModel.ConnectionDownQueueStrategy);
            InitializeConnection(_connectionFactory, timeout);
            RegisterAutoSubscriptions(_configurationModel);
        }

        void InitializeConnection(ConnectionFactory connectionFactory, TimeSpan timeout)
        {
            var timeoutInterval = TimeSpan.FromSeconds(10);
            IConnection connection = null;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (connection == null)
            {
                Logger.Current.Write("Initializing connection ...", TraceEventType.Information);

                try
                {
                    connection = connectionFactory.CreateConnection();
                    // ------------------------------------------------------------------------------------------
                    // Closing/disposing channels on IConnection.ConnectionShutdown causes a deadlock, so
                    // the ISession.SessionShutdown event is used here to infer a connection shutdown. 
                    // ------------------------------------------------------------------------------------------
                    ((ConnectionBase)connection).m_session0.SessionShutdown += UnexpectedConnectionShutdown;
                    connection.CallbackException += ConnectionCallbackException;
                    _messagePublisher.SetConnection(connection);
                    _configurationModel.DefaultDeadLetterStrategy.SetConnection(connection);

                    Logger.Current.Write(new LogEntry
                                            {
                                                Message = string.Format("Connected to the RabbitMQ node on host:{0}, port:{1}.",
                                                                        connection.Endpoint.HostName, connection.Endpoint.Port)
                                            });

                    _connection = connection;
                    OnConnectionEstablished(EventArgs.Empty);
                }
                catch (BrokerUnreachableException)
                {
                    OnConnectionFailed(EventArgs.Empty);
                    Logger.Current.Write(string.Format("The connection initialization failed because the RabbitMQ broker was unavailable. Reattempting connection in {0} seconds.",
                        timeoutInterval.Seconds), TraceEventType.Warning);
                    TimeProvider.Current.Sleep(timeoutInterval);
                }

                if (stopwatch.Elapsed > timeout)
                {
                    break;
                }
            }

            if (connection == null)
            {
                Logger.Current.Write("A connection to the RabbitMQ broker could not be established within the allotted time frame", TraceEventType.Critical);
            }
        }

        void UnexpectedConnectionShutdown(ISession session, ShutdownEventArgs reason)
        {
            Logger.Current.Write("Connection was shut down.", TraceEventType.Information);
            ((ConnectionBase)_connection).m_session0.SessionShutdown -= UnexpectedConnectionShutdown;

            lock (_connectionLock)
            {
                if (_closed) return;
                if (Reconnect(TimeSpan.FromSeconds(10)))
                {
                    RenewSubscriptions(_subscriptions.Values);
                    _messagePublisher.Flush();
                }
            }
        }

        void ConnectionCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            Logger.Current.Write("CallbackException received: " + e.Exception.Message, TraceEventType.Information);
        }

        void RenewSubscriptions(IEnumerable<ISubscription> subscriptions)
        {
            Logger.Current.Write("Renewing subscriptions ...", TraceEventType.Information);

            foreach (ISubscription subscription in subscriptions)
            {
                subscription.Renew(_connection);
            }
            Logger.Current.Write("Subscriptions have been renewed.", TraceEventType.Information);
        }

        void RemoveSubscriptions()
        {
            Logger.Current.Write("Removing subscriptions ...", TraceEventType.Information);

            foreach (ISubscriptionKey key in new List<ISubscriptionKey>(_subscriptions.Keys))
            {
                _subscriptions[key].Stop();
                _subscriptions.Remove(key);
            }
            Logger.Current.Write("Subscriptions have been removed.", TraceEventType.Information);
        }

        bool Reconnect(TimeSpan timeSpan)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (!_connection.IsOpen)
            {
                try
                {
                    Logger.Current.Write(string.Format("Attempting reconnect with last known configuration in {0} seconds.",
                                                       timeSpan.ToString("ss")), TraceEventType.Information);
                    TimeProvider.Current.Sleep(_configurationModel.ReconnectionInterval);
                    InitializeConnection(_connectionFactory, TimeSpan.MinValue);
                }
                catch (Exception)
                {
                    Logger.Current.Write("Connection failed.", TraceEventType.Information);
                }

                if (stopwatch.Elapsed > _configurationModel.ReconnectionTimeout)
                {
                    Logger.Current.Write("Timeout elapsed for reconnection attempts.", TraceEventType.Error);
                    OnConnectionTimeout(EventArgs.Empty);
                    return false;
                }
            }

            return true;
        }

        public void Close()
        {
            lock (_connectionLock)
            {
                if (_connection != null)
                {
                    ((ConnectionBase)_connection).m_session0.SessionShutdown -= UnexpectedConnectionShutdown;
                    if (_connection != null && _connection.IsOpen)
                    {
                        _connection.Close();
                        RemoveSubscriptions();
                        string message = string.Format("Disconnected from the RabbitMQ node on host:{0}, port:{1}.",
                                                       _connection.Endpoint.HostName, _connection.Endpoint.Port);
                        Logger.Current.Write(new LogEntry { Message = message });
                    }
                    _closed = true;
                }
            }
        }

        void SubscribeMessage<TMessage>(Action<IMessageContext<TMessage>> action, string routingKey, IDictionary arguments)
        {
            IConsumeInfo routeInfo = _configurationModel.ConsumeRouteConfiguration.GetRouteInfo(typeof(TMessage));
            var subscription = new Subscription<TMessage>(_connection, _configurationModel.DefaultDeadLetterStrategy,
                                                          _configurationModel.DefaultSerializationStrategy,
                                                          routeInfo, routingKey, action, arguments, _defaultErrorCallback,
                                                          _messagePublisher, SubscriptionType.Subscription, TimeSpan.MinValue);
            _subscriptions.Add(new SubscriptionKey(typeof(TMessage), routingKey, arguments), subscription);
            subscription.Start();
        }

        static void OnConsumeError(IErrorContext errorContext)
        {
            errorContext.RejectMessage(false);
        }

        public event EventHandler ConnectionEstablished;

        protected void OnConnectionEstablished(EventArgs e)
        {
            EventHandler handler = ConnectionEstablished;
            if (handler != null) handler(this, e);
        }

        public event EventHandler ConnectionFailed;

        protected void OnConnectionFailed(EventArgs e)
        {
            EventHandler handler = ConnectionFailed;
            if (handler != null) handler(this, e);
        }

        public event EventHandler ConnectionTimeout;

        protected void OnConnectionTimeout(EventArgs e)
        {
            EventHandler handler = ConnectionTimeout;
            if (handler != null) handler(this, e);
        }

        ~Bus()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free managed
                }
                Close();
                _disposed = true;
            }
        }
    }
}