using System;

namespace RabbitBus.Configuration
{
	public interface IQueueInfo
	{
		bool IsDurable { get; set; }
		bool IsAutoDelete { get; set; }
		bool IsAutoAcknowledge { get; set; }
		bool IsExclusive { get; set;  }
		INegatableQueueConfiguration Not { get; }
		TimeSpan? Expiration { get; }
	}
}