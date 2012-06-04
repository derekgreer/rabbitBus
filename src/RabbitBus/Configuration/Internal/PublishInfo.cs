using System.Collections;

namespace RabbitBus.Configuration.Internal
{
	class PublishInfo : IPublishInfo
	{
		public PublishInfo()
		{
			IsAutoDelete = true;
			ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
			DefaultRoutingKey = string.Empty;
			ReplyInfo = new ConsumeInfo();
		}

		public string ExchangeName { get; set; }
		public bool IsAutoDelete { get; set; }
		public bool IsDurable { get; set; }
		public bool IsPersistent { get; set; }
		public string ExchangeType { get; set; }
		public ISerializationStrategy SerializationStrategy { get; set; }
		public string DefaultRoutingKey { get; set; }
		public bool IsSigned { get; set; }
		public IConsumeInfo ReplyInfo { get; set; }
		public IDictionary DefaultHeaders { get; set; }
	}
}