using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace RabbitBus.Utilities
{
	public static class Declare
	{
		public static void Queue(string name)
		{
			Queue(name, ctx => { });
		}

		public static void Queue(string name, Action<IQueueDeclareContext> configure)
		{
			var queueDeclareInfo = new QueueDeclareInfo(name);
			configure(queueDeclareInfo);
			DeclareQueue(queueDeclareInfo);
		}

		static void DeclareQueue(IQueueDeclareInfo queueDeclareInfo)
		{
			var connectionFactory = new ConnectionFactory {Uri = queueDeclareInfo.Uri};
			IConnection connection = connectionFactory.CreateConnection();
			IModel channel = connection.CreateModel();

			IDictionary arguments = null;

			if (queueDeclareInfo.DeadLetterConfiguration != null)
			{
				arguments = new Dictionary<string, object>();
				arguments.Add("x-dead-letter-exchange", queueDeclareInfo.DeadLetterConfiguration.ExchangeName);

				if (!string.IsNullOrWhiteSpace(queueDeclareInfo.DeadLetterConfiguration.RoutingKey))
				{
					arguments.Add("x-dead-letter-routing-key", queueDeclareInfo.DeadLetterConfiguration.RoutingKey);
				}
			}

			if (queueDeclareInfo.Expiration.HasValue)
			{
				arguments = arguments ?? new Dictionary<string, object>();
				arguments.Add("x-message-ttl", Convert.ToInt32(queueDeclareInfo.Expiration.Value.TotalMilliseconds));
			}

			channel.ExchangeDeclare(queueDeclareInfo.Exchange.Name, queueDeclareInfo.Exchange.ExchangeType,
			                        queueDeclareInfo.Exchange.IsDurable, queueDeclareInfo.Exchange.IsAutoDelete, null);
			channel.QueueDeclare(queueDeclareInfo.Name, queueDeclareInfo.IsDurable, queueDeclareInfo.IsExclusive,
			                     queueDeclareInfo.IsAutoDelete, arguments);
			channel.QueueBind(queueDeclareInfo.Name, queueDeclareInfo.Name, queueDeclareInfo.RoutingKey, queueDeclareInfo.Headers);
			channel.Close();
			connection.Close();
		}
	}
}