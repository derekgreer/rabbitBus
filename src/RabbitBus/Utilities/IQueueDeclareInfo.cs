using System;
using System.Collections;
using System.Collections.Generic;
using RabbitBus.Configuration;

namespace RabbitBus.Utilities
{
	public interface IQueueDeclareInfo
	{
		string Uri { get; }
		string Name { get; }
		string RoutingKey { get; }
		bool IsAutoDelete { get; set; }
		bool IsDurable { get; set; }
		bool IsExclusive { get; set; }
		IExchangeDeclareInfo Exchange { get; }
		TimeSpan? Expiration { get; }
		IDeadLetterConfiguration DeadLetterConfiguration { get; }
		IDictionary<string, object> Headers { get; }
	}
}