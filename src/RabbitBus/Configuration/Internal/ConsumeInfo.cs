using System;

namespace RabbitBus.Configuration.Internal
{
	class ConsumeInfo : IConsumeInfo
	{
		public ConsumeInfo()
		{
			DefaultRoutingKey = string.Empty;
			IsExchangeAutoDelete = true;
			IsQueueAutoDelete = true;
			ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
		}

		public string ExchangeName { get; set; }
		public string QueueName { get; set; }
		public string DefaultRoutingKey { get; set; }
		public bool Exclusive { get; set; }
		public bool IsAutoAcknowledge { get; set; }
		public bool IsQueueAutoDelete { get; set; }
		public bool IsExchangeAutoDelete { get; set; }
		public bool IsQueueDurable { get; set; }
		public bool IsExchangeDurable { get; set; }
		public string ExchangeType { get; set; }
		public ISerializationStrategy SerializationStrategy { get; set; }
		public Action<IErrorContext> ErrorCallback { get; set; }
		public ushort QualityOfService { get; set; }
	}
}