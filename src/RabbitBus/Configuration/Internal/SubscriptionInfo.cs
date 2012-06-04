using System;
using System.Collections;
using RabbitMQ.Client;

namespace RabbitBus.Configuration.Internal
{
	class SubscriptionInfo<TMessage>
	{
		public IConnection Connection { get; set; }
		public IDeadLetterStrategy DeadLetterStrategy { get; set; }
		public ISerializationStrategy DefaultSerializationStrategy { get; set; }
		public IConsumeInfo ConsumeInfo { get; set; }
		public string RoutingKey { get; set; }
		public Action<IMessageContext<TMessage>> Callback { get; set; }
		public IDictionary ExchangeArguments { get; set; }
		public Action<IErrorContext> DefaultErrorCallback { get; set; }
		public IMessagePublisher MessagePublisher { get; set; }
		public SubscriptionType SubscriptionType { get; set; }
	}
}