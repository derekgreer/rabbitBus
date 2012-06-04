using System;
using System.Collections;
using System.IO;
using RabbitBus.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus.Specs.Infrastructure
{
	public class RabbitQueue
	{
		readonly IModel _channel;
		readonly IConnection _connection;
		readonly string _queueName;

		public RabbitQueue(string exchangeName, string queueName)
			: this("localhost", exchangeName, ExchangeType.Direct, queueName, false, true, false, true, "", null)
		{
		}

		public RabbitQueue(string exchangeName, string queueName, IDictionary headers)
			: this("localhost", exchangeName, ExchangeType.Direct, queueName, false, true, false, true, "", headers)
		{
		}

		public RabbitQueue(string host, string exchangeName, string exchangeType, string queueName) :
			this(host, exchangeName, exchangeType, queueName, false, true, false, true, "", null)
		{
		}

		public RabbitQueue(string host, string exchangeName, string exchangeType, string queueName, string routingKey) :
			this(host, exchangeName, exchangeType, queueName, false, true, false, true, routingKey, null)
		{
		}

		public RabbitQueue(string host, string exchangeName, string exchangeType, string queueName, bool durableExchange,
		                   bool autoDeleteExchange, bool durableQueue, bool autoDeleteQueue) :
		                   	this(host, exchangeName, exchangeType, queueName, durableExchange, autoDeleteExchange,
		                   	     durableQueue, autoDeleteQueue, string.Empty, null)
		{
		}

		public RabbitQueue(string host, string exchangeName, string exchangeType, string queueName, bool durableExchange,
		                   bool autoDeleteExchange, bool durableQueue, bool autoDeleteQueue, string routingKey,
		                   IDictionary headers)
		{
			_queueName = queueName;
			var connectionFactory = new ConnectionFactory {HostName = host};
			_connection = connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();

			if (exchangeName != string.Empty)
			{
				_channel.ExchangeDeclare(exchangeName, exchangeType, durableExchange, autoDeleteExchange, null);
				_channel.QueueDeclare(queueName, durableQueue, false, autoDeleteQueue, null);
				_channel.QueueBind(queueName, exchangeName, routingKey, headers);
			}
			else
			{
				_channel.QueueDeclare(queueName, durableQueue, false, autoDeleteQueue, null);
			}
		}

		public TMessage GetMessage<TMessage>() where TMessage : class
		{
			return GetMessage<TMessage>(new BinarySerializationStrategy(), true);
		}

		public TMessage GetMessage<TMessage>(ISerializationStrategy serializationStrategy)
			where TMessage : class
		{
			return GetMessage<TMessage>(serializationStrategy, true);
		}

		public TMessage GetMessage<TMessage>(ISerializationStrategy serializationStrategy, bool retry)
			where TMessage : class
		{
			TMessage message = default(TMessage);

			if (!retry)
			{
				Console.WriteLine("Getting message ...");
				BasicGetResult args = _channel.BasicGet(_queueName, true);
				if (args != null)
					message = new BinarySerializationStrategy().Deserialize<TMessage>(args.Body);
				return message;
			}

			new Action(() =>
				{
					try
					{
						var consumer = new QueueingBasicConsumer(_channel);
						Console.WriteLine("Consuming messages ...");
						_channel.BasicConsume(_queueName, true, "", null, consumer);
						var args = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
						message = serializationStrategy.Deserialize<TMessage>(args.Body);
					}
					catch (EndOfStreamException e)
					{
					}
				}).Background().BlockUntil(() => message != null).Then(() => _channel.Close())();

			return message;
		}

		public IBasicProperties GetMessageProperties<TMessage>() where TMessage : class
		{
			return GetMessageProperties<TMessage>(new BinarySerializationStrategy());
		}

		public IBasicProperties GetMessageProperties<TMessage>(ISerializationStrategy serializationStrategy)
			where TMessage : class
		{
			TMessage message = default(TMessage);
			BasicDeliverEventArgs args = null;

			new Action(() =>
				{
					try
					{
						var consumer = new QueueingBasicConsumer(_channel);
						_channel.BasicConsume(_queueName, true, "", null, consumer);
						args = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
						message = serializationStrategy.Deserialize<TMessage>(args.Body);
					}
					catch (EndOfStreamException e)
					{
					}
				}).Background().BlockUntil(() => message != null).Then(() => _channel.Close())();

			return args.BasicProperties;
		}

		public void Close()
		{
			_channel.Close();
			_connection.Close();
		}

		public RabbitQueue Empty()
		{
			while (_channel.BasicGet(_queueName, true) != null) ;
			return this;
		}

		public RabbitQueue Delete()
		{
			_channel.QueueDelete(_queueName, true, false);
			return this;
		}

		public static bool QueueExists(string queueName)
		{
			var connectionFactory = new ConnectionFactory {HostName = "localhost"};
			IConnection connection = connectionFactory.CreateConnection();
			IModel channel = connection.CreateModel();

			try
			{
				QueueDeclareOk queueDeclareOk = channel.QueueDeclarePassive(queueName);
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}
	}

	class RabbitDeadLetterQueue : RabbitQueue
	{
		public RabbitDeadLetterQueue()
			: base("localhost", string.Empty, ExchangeType.Direct, "deadletter", true, false, true, false, "deadletter", null)
		{
		}

		public RabbitDeadLetterQueue(string host, string exchangeName, string exchangeType, string queueName)
			: base("localhost", string.Empty, ExchangeType.Direct, "deadletter", true, false, true, false, "deadletter", null)
		{
		}

		public RabbitDeadLetterQueue(string host, string exchangeName, string exchangeType, string queueName,
		                             string routingKey)
			: base("localhost", string.Empty, ExchangeType.Direct, "deadletter", true, false, true, false, "deadletter", null)
		{
		}

		public RabbitDeadLetterQueue(string host, string exchangeName, string exchangeType, string queueName,
		                             bool durableExchange, bool autoDeleteExchange, bool durableQueue, bool autoDeleteQueue)
			: base("localhost", string.Empty, ExchangeType.Direct, "deadletter", true, false, true, false, "deadletter", null)

		{
		}

		public RabbitDeadLetterQueue(string host, string exchangeName, string exchangeType, string queueName,
		                             bool durableExchange, bool autoDeleteExchange, bool durableQueue, bool autoDeleteQueue,
		                             string routingKey)
			: base("localhost", string.Empty, ExchangeType.Direct, "deadletter", true, false, true, false, "deadletter", null)
		{
		}

		public RabbitDeadLetterQueue(string queueName)
			: base("localhost", string.Empty, ExchangeType.Direct, queueName, true, false, true, false, "deadletter", null)
		{
		}
	}
}