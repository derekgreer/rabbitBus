using System;
using System.Collections;
using System.Collections.Generic;
using RabbitBus.Configuration;
using RabbitMQ.Client;

namespace RabbitBus.Utilities
{
	public class QueueDeclareInfo : IQueueDeclareContext, IQueueDeclareInfo
	{
		public QueueDeclareInfo(string name)
		{
			Name = name;

			// Defaults
			Uri = "amqp://guest:guest@localhost:5672/%2f";
			RoutingKey = string.Empty;
			IsAutoDelete = true;
			IsDurable = false;
			IsExclusive = false;
			Exchange = new ExchangeDeclareContext
				{
					Name = name,
					ExchangeType = ExchangeType.Direct,
					IsAutoDelete = true,
					IsDurable = false
				};
		}

		public bool IsAutoDelete { get; private set; }

		public bool IsDurable { get; private set; }
		
		public string RoutingKey { get; set; }

		public IQueueDeclareContext AutoDelete()
		{
			IsAutoDelete = true;
			return this;
		}

		public INegatableQueueDeclareContext Not
		{
			get { return new NegatableQueueDeclareContext(this); }
		}

		public IQueueDeclareContext WithExchange(string exchangeName)
		{
			return WithExchange(exchangeName, ctx => { });
		}

		public IQueueDeclareContext WithExchange(string exchangeName, Action<IExchangeDeclareContext> configure)
		{
			var exchangeDeclareContext = new ExchangeDeclareContext();
			exchangeDeclareContext.Name = exchangeName;
			configure(exchangeDeclareContext);
			return this;
		}

		public IQueueDeclareContext WithExpiration(TimeSpan expirationTimeSpan)
		{
			Expiration = expirationTimeSpan;
			return this;
		}

		public IQueueDeclareContext WithDeadLetterExchange(string deadLetterExchange)
		{
			return WithDeadLetterExchange(deadLetterExchange, string.Empty);
		}

		public IQueueDeclareContext WithDeadLetterExchange(string deadLetterExchange, string deadLetterRoutingKey)
		{
			DeadLetterConfiguration = new DeadLetterConfiguration(deadLetterExchange, deadLetterRoutingKey);
			return this;
		}

		public IQueueDeclareContext WithRoutingKey(string routingKey)
		{
			RoutingKey = routingKey;
			return this;
		}

		public IQueueDeclareContext WithHeaders(IDictionary<string, object> dictionary)
		{
			Headers = dictionary;
			return this;
		}

		public IDictionary<string, object> Headers { get; private set; }

		public IDeadLetterConfiguration DeadLetterConfiguration { get; private set; }

		public TimeSpan? Expiration { get; private set; }

		public bool IsExclusive { get; set; }

		public IExchangeDeclareInfo Exchange { get; set; }

		public string Uri { get; private set; }

		public string Name { get; private set; }

		bool IQueueDeclareInfo.IsAutoDelete
		{
			get { return IsAutoDelete; }
			set { IsAutoDelete = value; }
		}

		bool IQueueDeclareInfo.IsDurable
		{
			get { return IsDurable; }
			set { IsDurable = value; }
		}
	}

	class NegatableQueueDeclareContext : INegatableQueueDeclareContext
	{
		readonly IQueueDeclareInfo _queueDeclareInfo;

		public NegatableQueueDeclareContext(IQueueDeclareInfo queueDeclareInfo)
		{
			_queueDeclareInfo = queueDeclareInfo;
		}

		public IQueueDeclareContext AutoDelete()
		{
			_queueDeclareInfo.IsAutoDelete = false;
			return (IQueueDeclareContext) _queueDeclareInfo;
		}

		public IQueueDeclareContext Durable()
		{
			_queueDeclareInfo.IsDurable = false;
			return (IQueueDeclareContext) _queueDeclareInfo;
		}

		public IQueueDeclareContext Exclusive()
		{
			_queueDeclareInfo.IsExclusive = false;
			return (IQueueDeclareContext) _queueDeclareInfo;
		}
	}
}