#region Usings

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

#endregion

namespace RabbitBus
{
    internal class Subscription<TMessage> : ISubscription
    {
        private readonly Action<IMessageContext<TMessage>> _callback;
        private readonly TimeSpan _callbackTimeout;
        private readonly IConsumeInfo _consumeInfo;
        private readonly IDeadLetterStrategy _deadLetterStrategy;
        private readonly Action<IErrorContext> _defaultErrorCallback;
        private readonly ISerializationStrategy _defaultSerializationStrategy;
        private readonly IDictionary _exchangeArguments;
        private readonly IMessagePublisher _messagePublisher;
        private readonly string _routingKey;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly SubscriptionType _subscriptionType;
        private IConnection _connection;
        private QueueingBasicConsumer _consumer;
        private Task _task;
        private bool _threadCancelled;


        public event EventHandler Stopped;

        public Subscription(IConnection connection, IDeadLetterStrategy deadLetterStrategy,
                            ISerializationStrategy defaultSerializationStrategy, IConsumeInfo consumeInfo,
                            string routingKey,
                            Action<IMessageContext<TMessage>> callback, IDictionary exchangeArguments,
                            Action<IErrorContext> defaultErrorCallback, IMessagePublisher messagePublisher,
                            SubscriptionType subscriptionType, TimeSpan callbackTimeout)
        {
            _connection = connection;
            _deadLetterStrategy = deadLetterStrategy;
            _defaultSerializationStrategy = defaultSerializationStrategy;
            _consumeInfo = consumeInfo;
            _routingKey = routingKey ?? _consumeInfo.DefaultRoutingKey;
            _callback = callback;
            _defaultErrorCallback = defaultErrorCallback;
            _messagePublisher = messagePublisher;
            _subscriptionType = subscriptionType;
            _callbackTimeout = callbackTimeout;
            _exchangeArguments = exchangeArguments;
        }

        public void Start()
        {
            try
            {
                var channel = _connection.CreateModel();
                channel.ModelShutdown += ChannelModelShutdown;

                if (_consumeInfo.ExchangeName != string.Empty)
                {
                    channel.ExchangeDeclare(_consumeInfo.ExchangeName, _consumeInfo.ExchangeType,
                                            _consumeInfo.IsExchangeDurable,
                                            _consumeInfo.IsExchangeAutoDelete, null);
                }
                channel.QueueDeclare(_consumeInfo.QueueName, _consumeInfo.IsQueueDurable, _consumeInfo.Exclusive, _consumeInfo.IsQueueAutoDelete, _exchangeArguments);
                if (_consumeInfo.ExchangeName != string.Empty)
                {
                    channel.QueueBind(_consumeInfo.QueueName, _consumeInfo.ExchangeName, _routingKey, _exchangeArguments);
                }

                _consumer = new QueueingBasicConsumer(channel);
                channel.BasicQos(0, _consumeInfo.QualityOfService, false);
                channel.BasicConsume(_consumeInfo.QueueName, _consumeInfo.IsAutoAcknowledge, _consumer);
                _task = new Task(() => Subscribe(channel));
                _task.Start();

                var log =
                    string.Format(
                        "Subscribed to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
                        _connection.Endpoint.HostName,
                        _connection.Endpoint.Port,
                        _consumeInfo.ExchangeName,
                        _consumeInfo.QueueName,
                        _routingKey);
                Logger.Current.Write(new LogEntry {Message = log});
            }
            catch (Exception e)
            {
                Logger.Current.Write("An exception occurred starting the subscription: " + e.Message,
                                     TraceEventType.Error);
            }
        }

        public void Stop()
        {
            var log =
                string.Format(
                    "Stopping subscription to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
                    _connection.Endpoint.HostName,
                    _connection.Endpoint.Port,
                    _consumeInfo.ExchangeName,
                    _consumeInfo.QueueName,
                    _routingKey);
            Logger.Current.Write(log, TraceEventType.Information);
            _threadCancelled = true;
            _task.Wait();
            _threadCancelled = false;
            _task = null;
        }

        public void Renew(IConnection connection)
        {
            Stop();
            _connection = connection;
            Start();
        }

        private void Subscribe(IModel channel)
        {            
            var logger = Logger.Current;
            _stopwatch.Start();            

            var log =
                string.Format(
                    "Starting thread for subscription to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
                    _connection.Endpoint.HostName,
                    _connection.Endpoint.Port,
                    _consumeInfo.ExchangeName,
                    _consumeInfo.QueueName,
                    _routingKey);
            logger.Write(log, TraceEventType.Information);

            while (true)
            {
                if (WaitExceeded() || _threadCancelled)
                {
                    break;
                }

                BasicDeliverEventArgs eventArgs = null;

                try
                {
                    object eArgs;
                    _consumer.Queue.Dequeue(1000, out eArgs);

                    if (eArgs != null)
                    {
                        eventArgs = (BasicDeliverEventArgs) eArgs;
                        logger.Write(string.Format("Message received: {0} bytes", eventArgs.Body.Length),
                                     TraceEventType.Information);
                        var serializationStrategy = _consumeInfo.SerializationStrategy ??
                                                    _defaultSerializationStrategy;
                        object message = serializationStrategy.Deserialize<TMessage>(eventArgs.Body);

                        var messageContext = new MessageContext<TMessage>(_deadLetterStrategy, (TMessage) message,
                                                                          _consumeInfo,
                                                                          channel,
                                                                          eventArgs.DeliveryTag, eventArgs.Redelivered,
                                                                          eventArgs.Exchange, eventArgs.RoutingKey,
                                                                          eventArgs.BasicProperties, eventArgs.Body,
                                                                          _messagePublisher);

                        _callback(messageContext);

                        if (_subscriptionType == SubscriptionType.RemoteProcedure)
                        {
                            log =
                                string.Format(
                                    "Terminating RPC subscription to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
                                    _connection.Endpoint.HostName,
                                    _connection.Endpoint.Port,
                                    _consumeInfo.ExchangeName,
                                    _consumeInfo.QueueName,
                                    _routingKey);
                            logger.Write(log, TraceEventType.Information);
                            break;
                        }
                    }
                }
                catch (EndOfStreamException)
                {
                    logger.Write("Received EndOfStreamException.", TraceEventType.Information);
                    InvokeErrorCallback(eventArgs, channel);
                    channel.Dispose();
                    channel = null;
                    logger.Write("Subscription terminated.", TraceEventType.Information);
                    break;
                }
                catch (AlreadyClosedException e)
                {
                    Logger.Current.Write(
                        string.Format("An AlreadyClosedException occurred: {0} {1}", e.Message, e.StackTrace),
                        TraceEventType.Error);
                    InvokeErrorCallback(eventArgs, channel);
                    break;
                }
                catch (Exception e)
                {
                    Logger.Current.Write("An exception occurred while dequeuing a message: " + e.Message,
                                         TraceEventType.Error);
                    InvokeErrorCallback(eventArgs, channel);
                }                
            }    
        
            // cleaning channel
            if (channel!= null && channel.IsOpen)
            {
                channel.Close();
            }

            if (Stopped != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        private void ChannelModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            try
            {
                Logger.Current.Write("Closing the channel ... ", TraceEventType.Information);
                model.Close();
                _threadCancelled = true;
                Logger.Current.Write("Channel closed.", TraceEventType.Information);
            }
            catch (Exception e)
            {
                Logger.Current.Write("An exception occurred closing the channel: " + e.Message, TraceEventType.Error);
            }
        }

        private void InvokeErrorCallback(BasicDeliverEventArgs eventArgs, IModel channel)
        {
            try
            {
                var errorCallback = _consumeInfo.ErrorCallback ?? _defaultErrorCallback;
                errorCallback(new ErrorContext(channel, eventArgs));
            }
            catch (Exception exception)
            {
                Logger.Current.Write(
                    "An exception occurred invoking the registered error callback: " + exception.Message,
                    TraceEventType.Error);
            }
        }

        private bool WaitExceeded()
        {
            if (_callbackTimeout == TimeSpan.MinValue)
            {
                return false;
            }

            return _stopwatch.ElapsedTicks > _callbackTimeout.Ticks;
        }
    }

    public enum SubscriptionType
    {
        Subscription,
        RemoteProcedure
    }
}