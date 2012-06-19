using System;

namespace RabbitBus.Configuration.Internal
{
	class DefaultPublishRouteInfoLookupStrategy : IRouteInfoLookupStrategy<IPublishInfo>
	{
		public void Register(Type messageType, IPublishInfo routeInfo)
		{
			// no implementation
		}

		public IPublishInfo LookupRouteInfo(Type messageType)
		{
			string name = messageType.Name;
			return new PublishInfo {ExchangeName = name, IsDurable = false, IsAutoDelete = true};
		}
	}
}