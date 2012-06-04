using System;
using System.Collections;
using System.Diagnostics;
using RabbitBus.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace RabbitBus.Specs.Infrastructure
{
	public class RabbitExchange
	{
		readonly IModel _channel;
		readonly IConnection _connection;
		readonly string _exchangeName;
		readonly DateTime _unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);

		public RabbitExchange(string exchangeName)
			: this("localhost", exchangeName, ExchangeType.Direct, false, true)
		{
		}

		public RabbitExchange(string exchangeName, string exchangeType)
			: this("localhost", exchangeName, exchangeType, false, true)
		{
		}

		public RabbitExchange(string host, string exchangeName, string exchangeType)
			: this(host, exchangeName, exchangeType, false, true)
		{
		}

		public RabbitExchange(string host, string exchangeName, string exchangeType, bool durable, bool autoDelete)
		{
			_exchangeName = exchangeName;
			var connectionFactory = new ConnectionFactory {HostName = host};
			_connection = connectionFactory.CreateConnection();
			_channel = _connection.CreateModel();
			_channel.ExchangeDeclare(exchangeName, exchangeType, durable, autoDelete, null);
		}

		public void Publish<TMessage>(TMessage message)
		{
			Publish(message, string.Empty, new BinarySerializationStrategy());
		}

		public void Publish<TMessage>(TMessage message, string routingKey)
		{
			Publish(message, routingKey, new BinarySerializationStrategy());
		}

		public void Publish<TMessage>(TMessage message, string routingKey, ISerializationStrategy serializationStrategy)
		{
			Console.WriteLine(
				string.Format("Publishing message to exchange:\'{0}\' routing key:\'{1}\'", _exchangeName, routingKey),
				TraceEventType.Information);
			byte[] msg = serializationStrategy.Serialize(message);
			var properties = new BasicProperties();
			properties.Timestamp = new AmqpTimestamp((long) (DateTime.Now - _unixEpoch).TotalSeconds);
			_channel.BasicPublish(_exchangeName, routingKey, properties, msg);
		}

		public void Publish<TMessage>(TMessage message, IDictionary headers)
		{
			var properties = new BasicProperties();
			properties.Headers = headers;
			properties.Timestamp = new AmqpTimestamp((long) (DateTime.Now - _unixEpoch).TotalSeconds);
			var serializationStrategy = new BinarySerializationStrategy();
			byte[] msg = serializationStrategy.Serialize(message);
			_channel.BasicPublish(_exchangeName, string.Empty, properties, msg);
		}

		public void Close()
		{
			_channel.Close();
			_connection.Close();
		}

		public RabbitExchange Delete()
		{
			return Delete(true);
		}


		public RabbitExchange Delete(bool ifUsed)
		{
			try
			{
				Console.WriteLine("Deleting exchange: " + _exchangeName);
				_channel.ExchangeDelete(_exchangeName, ifUsed);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			return this;
		}
	}
}