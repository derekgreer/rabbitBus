using System;

namespace RabbitBus.Configuration.Internal
{
	class DefaultSubscriptionRouteInfoLookupStrategy : IRouteInfoLookupStrategy<IConsumeInfo>
	{
		public void Register(Type messageType, IConsumeInfo routeInfo)
		{
			// no implementation
		}

		public IConsumeInfo LookupRouteInfo(Type messageType)
		{
			string name = messageType.Name;
			return new ConsumeInfo
			       	{
			       		ExchangeName = name,
			       		IsExchangeDurable = false,
			       		IsExchangeAutoDelete = true,
			       		QueueName = name,
			       		IsQueueDurable = false,
			       		IsQueueAutoDelete = true
			       	};
		}
	}
}