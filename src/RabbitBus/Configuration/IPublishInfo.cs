using System;
using System.Collections;
using System.Collections.Generic;

namespace RabbitBus.Configuration
{
	public interface IPublishInfo
	{
		string ExchangeName { get; set; }
		bool IsAutoDelete { get; set; }
		bool IsDurable { get; set; }
		bool IsPersistent { get; set; }
		string ExchangeType { get; set; }
		ISerializationStrategy SerializationStrategy { get; set; }
		string DefaultRoutingKey { get; set; }
		bool IsSigned { get; set; }
		IConsumeInfo ReplyInfo { get; set; }
		IDictionary<string, object> DefaultHeaders { get; set; }
		TimeSpan? Expiration { get; set; }
	}
}