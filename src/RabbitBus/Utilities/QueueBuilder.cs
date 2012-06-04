using System;
using System.Collections;
using RabbitBus.Configuration;
using RabbitBus.Configuration.Internal;
using RabbitMQ.Client;

namespace RabbitBus.Utilities
{
	public class QueueBuilder
	{
		string _uri = "amqp://guest:guest@localhost:5672/%2f";
		string _exchangeName;
		string _exchangeType;
		IDictionary _headers;
		string _queueName;
		string _routingKey = string.Empty;

		public QueueBuilder WithExchange(string exchangeName, Action<IExchangeTypeConfiguration> exchangeConfiguration)
		{
			_exchangeName = exchangeName;
			var exchangeInfo = new ExchangeInfo();
			exchangeConfiguration(exchangeInfo);
			_exchangeType = exchangeInfo.ExchangeType;
			return this;
		}

		public QueueBuilder WithName(string queueName)
		{
			_queueName = queueName;
			return this;
		}

		public void Declare()
		{
			var connectionFactory = new ConnectionFactory {Uri = _uri};
			IConnection connection = connectionFactory.CreateConnection();
			IModel channel = connection.CreateModel();

			channel.ExchangeDeclare(_exchangeName, _exchangeType, true, false, null);
			channel.QueueDeclare(_queueName, true, false, false, null);
			channel.QueueBind(_queueName, _exchangeName, _routingKey, _headers);
			channel.Close();
			connection.Close();
		}

		public QueueBuilder WithRoutingKey(string routingKey)
		{
			_routingKey = routingKey;
			return this;
		}

		public QueueBuilder WithConnection(string amqpUri)
		{
			_uri = amqpUri;
			return this;
		}
	}
}