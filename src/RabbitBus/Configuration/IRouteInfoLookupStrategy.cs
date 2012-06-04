using System;

namespace RabbitBus.Configuration
{
	public interface IRouteInfoLookupStrategy<TRouteInfo>
	{
		void Register(Type messageType, TRouteInfo routeInfo);
		TRouteInfo LookupRouteInfo(Type messageType);
	}
}