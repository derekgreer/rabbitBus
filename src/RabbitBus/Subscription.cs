using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus
{
	public interface ISubscription
	{
		void Start();
		void Stop();
		void Renew(IConnection connection);
	}

	class Subscription<TMessage> : ISubscription
	{
		readonly Action<IMessageContext<TMessage>> _callback;
		readonly IConsumeInfo _consumeInfo;
		readonly IDeadLetterStrategy _deadLetterStrategy;
		readonly Action<IErrorContext> _defaultErrorCallback;
		readonly ISerializationStrategy _defaultSerializationStrategy;
		readonly IDictionary _exchangeArguments;
		readonly IMessagePublisher _messagePublisher;
		readonly string _routingKey;
		readonly SubscriptionType _subscriptionType;
		IModel _channel;
		IConnection _connection;
		Thread _thread;

		public Subscription(IConnection connection, IDeadLetterStrategy deadLetterStrategy,
		                    ISerializationStrategy defaultSerializationStrategy, IConsumeInfo consumeInfo, string routingKey,
		                    Action<IMessageContext<TMessage>> callback, IDictionary exchangeArguments,
		                    Action<IErrorContext> defaultErrorCallback, IMessagePublisher messagePublisher,
		                    SubscriptionType subscriptionType)
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
			_exchangeArguments = exchangeArguments;
		}

		public void Start()
		{
			try
			{
				ILogger logger = Logger.Current;
				_channel = _connection.CreateModel();
				if(_consumeInfo.ExchangeName != string.Empty)
				{
					_channel.ExchangeDeclare(_consumeInfo.ExchangeName, _consumeInfo.ExchangeType, _consumeInfo.IsExchangeDurable,
					                         _consumeInfo.IsExchangeAutoDelete, null);
				}
				_channel.QueueDeclare(_consumeInfo.QueueName, _consumeInfo.IsQueueDurable, _consumeInfo.Exclusive,
				                      _consumeInfo.IsQueueAutoDelete, _exchangeArguments);
				if(_consumeInfo.ExchangeName != string.Empty)
				{
					_channel.QueueBind(_consumeInfo.QueueName, _consumeInfo.ExchangeName, _routingKey, _exchangeArguments);
				}

				var consumer = new QueueingBasicConsumer(_channel);
				_channel.BasicQos(0, _consumeInfo.QualityOfService, false);
				_channel.BasicConsume(_consumeInfo.QueueName, _consumeInfo.IsAutoAcknowledge, consumer);

				string log;
				_thread = new Thread(() =>
					{
						while (true)
						{
							BasicDeliverEventArgs eventArgs = null;

							try
							{
								eventArgs = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
								logger.Write(string.Format("Message received: {0} bytes", eventArgs.Body.Length),
								             TraceEventType.Information);
								ISerializationStrategy serializationStrategy = _consumeInfo.SerializationStrategy ??
								                                               _defaultSerializationStrategy;
								object message = serializationStrategy.Deserialize<TMessage>(eventArgs.Body);

								var messageContext = new MessageContext<TMessage>(_deadLetterStrategy, (TMessage) message, _consumeInfo,
								                                                  _channel,
								                                                  eventArgs.DeliveryTag, eventArgs.Redelivered,
								                                                  eventArgs.Exchange, eventArgs.RoutingKey,
								                                                  eventArgs.BasicProperties, eventArgs.Body, _messagePublisher);

								_callback(messageContext);

								if (_subscriptionType == SubscriptionType.RemoteProcedure)
								{
									log = string.Format("Terminating RPC subscription to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
									                    _connection.Endpoint.HostName,
									                    _connection.Endpoint.Port,
									                    _consumeInfo.ExchangeName,
									                    _consumeInfo.QueueName,
									                    _routingKey);
									logger.Write(log, TraceEventType.Information);
									break;
								}
							}
							catch (EndOfStreamException e)
							{
								_channel.Dispose();
								_channel = null;
								logger.Write("Subscription terminated.", TraceEventType.Information);
								break;
							}
							catch (ThreadAbortException e)
							{
								Logger.Current.Write("The subscription thread was aborted.", TraceEventType.Error);
							}
							catch (Exception e)
							{
								Action<IErrorContext> errorCallback = _consumeInfo.ErrorCallback ?? _defaultErrorCallback;
								errorCallback(new ErrorContext(_channel, eventArgs));

								Logger.Current.Write("An exception occurred while dequeuing a message: " + e.Message, TraceEventType.Error);
							}
						}
						if (_channel != null && _channel.IsOpen) _channel.Close();
					});

				_thread.Start();

				log = string.Format("Subscribed to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
				                    _connection.Endpoint.HostName,
				                    _connection.Endpoint.Port,
				                    _consumeInfo.ExchangeName,
				                    _consumeInfo.QueueName,
				                    _routingKey);
				Logger.Current.Write(new LogEntry {Message = log});
			}
			catch (Exception e)
			{
				Logger.Current.Write("An exception occurred starting the subscription: " + e.Message, TraceEventType.Error);
			}
		}

		public void Stop()
		{
			if (_connection.IsOpen)
				_channel.Close();
		}

		public void Renew(IConnection connection)
		{
			_thread.Abort();
			_connection = connection;
			Start();
		}
	}

	public enum SubscriptionType
	{
		Subscription,
		RemoteProcedure
	}
}