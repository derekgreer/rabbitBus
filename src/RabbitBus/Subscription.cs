using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace RabbitBus
{
	class Subscription<TMessage> : ISubscription
	{
		readonly Action<IMessageContext<TMessage>> _callback;
		readonly TimeSpan _callbackTimeout;
		readonly IConsumeInfo _consumeInfo;
		readonly IDeadLetterConfiguration _defaultDeadLetterConfiguration;
		readonly Action<IErrorContext> _defaultErrorCallback;
		readonly ISerializationStrategy _defaultSerializationStrategy;
		readonly IMessagePublisher _messagePublisher;
		readonly IDictionary _queueProperties;
		readonly string _routingKey;
		readonly Stopwatch _stopwatch = new Stopwatch();
		readonly SubscriptionType _subscriptionType;
		IConnection _connection;

		QueueingBasicConsumer _consumer;
		Thread _thread;
		bool _threadCancelled;

		public Subscription(IConnection connection, IDeadLetterConfiguration defaultDeadLetterConfiguration,
		                    ISerializationStrategy defaultSerializationStrategy, IConsumeInfo consumeInfo,
		                    string routingKey,
		                    Action<IMessageContext<TMessage>> callback, IDictionary queueProperties,
		                    Action<IErrorContext> defaultErrorCallback, IMessagePublisher messagePublisher,
		                    SubscriptionType subscriptionType, TimeSpan callbackTimeout)
		{
			_connection = connection;
			_defaultDeadLetterConfiguration = defaultDeadLetterConfiguration;
			_defaultSerializationStrategy = defaultSerializationStrategy;
			_consumeInfo = consumeInfo;
			_routingKey = routingKey ?? _consumeInfo.DefaultRoutingKey;
			_callback = callback;
			_defaultErrorCallback = defaultErrorCallback;
			_messagePublisher = messagePublisher;
			_subscriptionType = subscriptionType;
			_callbackTimeout = callbackTimeout;
			_queueProperties = queueProperties;
		}

		public void Start()
		{
			try
			{
				IModel channel = _connection.CreateModel();
				channel.ModelShutdown += ChannelModelShutdown;

				if (_consumeInfo.ExchangeName != string.Empty)
				{
					channel.ExchangeDeclare(_consumeInfo.ExchangeName, _consumeInfo.ExchangeType,
					                        _consumeInfo.IsExchangeDurable,
					                        _consumeInfo.IsExchangeAutoDelete, null);
				}


				IDictionary queueDeclareArguments = null;
				queueDeclareArguments = AddDeadLetterArguments(queueDeclareArguments);
				queueDeclareArguments = AddExpirationArguments(queueDeclareArguments);

				channel.QueueDeclare(_consumeInfo.QueueName, _consumeInfo.IsQueueDurable, _consumeInfo.IsQueueExclusive,
				                     _consumeInfo.IsQueueAutoDelete, queueDeclareArguments);

				if (_consumeInfo.ExchangeName != string.Empty)
				{
					channel.QueueBind(_consumeInfo.QueueName, _consumeInfo.ExchangeName, _routingKey, _queueProperties);
				}

				_consumer = new QueueingBasicConsumer(channel);
				channel.BasicQos(0, _consumeInfo.QualityOfService, false);
				channel.BasicConsume(_consumeInfo.QueueName, _consumeInfo.IsAutoAcknowledge, _consumer);
				_thread = new Thread(() =>
				    {
				        Subscribe(channel);
                        channel.Dispose();
				    });
				_thread.Start();

				string log =
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
			string log =
				string.Format(
					"Stopping subscription to messages from host: {0}, port: {1}, exchange: {2}, queue: {3}, routingKey: {4}",
					_connection.Endpoint.HostName,
					_connection.Endpoint.Port,
					_consumeInfo.ExchangeName,
					_consumeInfo.QueueName,
					_routingKey);
			Logger.Current.Write(log, TraceEventType.Information);
			_threadCancelled = true;
			if (_thread != null)
				_thread.Join();
			_threadCancelled = false;
			_thread = null;
		}

		public void Renew(IConnection connection)
		{
			Stop();
			_connection = connection;
			Start();
		}

		IDictionary AddExpirationArguments(IDictionary exchangeArguments)
		{
			if (_consumeInfo.Expiration.HasValue)
			{
				if (exchangeArguments == null)
				{
					exchangeArguments = new Dictionary<string, object>();
				}

				exchangeArguments.Add("x-message-ttl", (int) _consumeInfo.Expiration.Value.TotalMilliseconds);
			}
			return exchangeArguments;
		}

		IDictionary AddDeadLetterArguments(IDictionary exchangeArguments)
		{
			IDictionary queueDeclareArgs = exchangeArguments;

			IDeadLetterConfiguration deadLetterConfig = _consumeInfo.DeadLetterConfiguration ?? _defaultDeadLetterConfiguration;

			if (deadLetterConfig != null)
			{
				string deadLetterExchangeName = deadLetterConfig.ExchangeName;
				string deadLetterRoutingKey = deadLetterConfig.RoutingKey;

				if (exchangeArguments == null)
				{
					queueDeclareArgs = new Dictionary<string, object>();
				}

				queueDeclareArgs.Add("x-dead-letter-exchange", deadLetterExchangeName);

				if (!string.IsNullOrWhiteSpace(deadLetterRoutingKey))
					queueDeclareArgs.Add("x-dead-letter-routing-key", deadLetterRoutingKey);
			}
			return queueDeclareArgs;
		}

		void Subscribe(IModel channel)
		{
			ILogger logger = Logger.Current;
			_stopwatch.Start();

			string log =
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
					object eArgs = null;
					_consumer.Queue.Dequeue(1000, out eArgs);

					if (eArgs != null)
					{
						eventArgs = (BasicDeliverEventArgs) eArgs;
						logger.Write(string.Format("Message received: {0} bytes", eventArgs.Body.Length), TraceEventType.Information);
						ISerializationStrategy serializationStrategy = _consumeInfo.SerializationStrategy ??
						                                               _defaultSerializationStrategy;
						object message = serializationStrategy.Deserialize<TMessage>(eventArgs.Body);

						var messageContext = new MessageContext<TMessage>( /*_deadLetterStrategy, */ (TMessage) message,
						                                                                             _consumeInfo,
						                                                                             channel,
						                                                                             eventArgs.DeliveryTag,
						                                                                             eventArgs.Redelivered,
						                                                                             eventArgs.Exchange,
						                                                                             eventArgs.RoutingKey,
						                                                                             eventArgs.BasicProperties,
						                                                                             eventArgs.Body,
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
					Logger.Current.Write(string.Format("An AlreadyClosedException occurred: {0} {1}", e.Message, e.StackTrace),
					                     TraceEventType.Error);
					InvokeErrorCallback(eventArgs, channel);
					break;
				}
				catch (Exception e)
				{
					Logger.Current.Write("An exception occurred while dequeuing a message: " + e.Message, TraceEventType.Error);
					InvokeErrorCallback(eventArgs, channel);
				}
			}
		}
	
		void ChannelModelShutdown(IModel model, ShutdownEventArgs reason)
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

		void InvokeErrorCallback(BasicDeliverEventArgs eventArgs, IModel channel)
		{
			try
			{
				Action<IErrorContext> errorCallback = _consumeInfo.ErrorCallback ?? _defaultErrorCallback;
				errorCallback(new ErrorContext(channel, eventArgs /*, _deadLetterStrategy */));
			}
			catch (Exception exception)
			{
				Logger.Current.Write(
					"An exception occurred invoking the registered error callback: " + exception.Message,
					TraceEventType.Error);
			}
		}

		bool WaitExceeded()
		{
			if (_callbackTimeout == TimeSpan.MinValue)
			{
				return false;
			}

			return _stopwatch.ElapsedTicks > _callbackTimeout.Ticks;
		}

		public event EventHandler<EventArgs> Stopped;


	}

	public enum SubscriptionType
	{
		Subscription,
		RemoteProcedure
	}
}