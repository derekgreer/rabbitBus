using System.Diagnostics;
using System.Globalization;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitBus.Logging;
using RabbitMQ.Client;

namespace RabbitBus
{
	public class ConsumerContext<TMessage> : IConsumerContext<TMessage>
	{
		readonly IModel _channel;
		readonly IConsumeInfo _consumeInfo;
		readonly ISerializationStrategy _defaultSerializationStrategy;
		readonly IDeadLetterStrategy _deadLetterStrategy;
		readonly IMessagePublisher _messagePublisher;

		public ConsumerContext(IConnection connection, IConsumeInfo consumeInfo, ISerializationStrategy defaultSerializationStrategy, IDeadLetterStrategy deadLetterStrategy, IMessagePublisher messagePublisher)
		{
			_consumeInfo = consumeInfo;
			_defaultSerializationStrategy = defaultSerializationStrategy;
			_deadLetterStrategy = deadLetterStrategy;
			_messagePublisher = messagePublisher;
			_channel = connection.CreateModel();

			Logger.Current.Write(string.Format("Declaring exchange:\'{0}\' durable:{1} auto-delete:{2}",
				consumeInfo.ExchangeName, 
				consumeInfo.IsExchangeDurable.ToString(CultureInfo.InvariantCulture),
				consumeInfo.IsExchangeAutoDelete.ToString(CultureInfo.InvariantCulture)),
				TraceEventType.Information);
			_channel.ExchangeDeclare(consumeInfo.ExchangeName, consumeInfo.ExchangeType, consumeInfo.IsExchangeDurable,
			                         consumeInfo.IsExchangeAutoDelete, null);
			Logger.Current.Write(string.Format("Declaring queue:\'{0}\' durable:{1} auto-delete:{2}",
				consumeInfo.QueueName,
				consumeInfo.IsQueueDurable.ToString(CultureInfo.InvariantCulture),
				consumeInfo.IsQueueAutoDelete.ToString(CultureInfo.InvariantCulture)),
				TraceEventType.Information);
			_channel.QueueDeclare(consumeInfo.QueueName, consumeInfo.IsQueueDurable, consumeInfo.Exclusive,
			                      consumeInfo.IsQueueAutoDelete, null);
			Logger.Current.Write(string.Format("Binding queue \'{0}\' to exchange \'{1}\'", consumeInfo.QueueName, consumeInfo.ExchangeName), TraceEventType.Information);
			_channel.QueueBind(consumeInfo.QueueName, consumeInfo.ExchangeName, consumeInfo.DefaultRoutingKey);
		}

		public ConsumerContext(IModel channel, IConsumeInfo consumeInfo)
		{
			_channel = channel;
			_consumeInfo = consumeInfo;
		}

		public void Dispose()
		{
			_channel.Close();
		}

		public IMessageContext<TMessage> GetMessage()
		{
			Logger.Current.Write(string.Format("Pulling message from queue:\'{0}\' auto-acknowledge:{1}",
				_consumeInfo.QueueName, _consumeInfo.IsAutoAcknowledge), TraceEventType.Information);
			IMessageContext<TMessage> messageContext = null;
			BasicGetResult result = _channel.BasicGet(_consumeInfo.QueueName, _consumeInfo.IsAutoAcknowledge);

			if (result != null)
			{
				ISerializationStrategy serializationStrategy = _consumeInfo.SerializationStrategy ?? _defaultSerializationStrategy;
				object message = serializationStrategy.Deserialize<TMessage>(result.Body);
				messageContext = new MessageContext<TMessage>(_deadLetterStrategy, (TMessage) message, _consumeInfo, _channel, result.DeliveryTag,
				                                              result.Redelivered, result.Exchange, result.RoutingKey,
				                                              result.BasicProperties, result.Body, _messagePublisher);
			}

			return messageContext;
		}
	}
}