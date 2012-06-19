using System;

namespace RabbitBus.Configuration
{
	public interface IRouteInfoLookupPolicy
	{
	}

	class MapRouteInfoLookupPolicy<TRouteInfo> : IRouteInfoLookupPolicy
	{
		public Type MessageType { get; set; }
		public TRouteInfo RouteInfo { get; set; }

		public MapRouteInfoLookupPolicy(Type messageType, TRouteInfo routeInfo)
		{
			MessageType = messageType;
			RouteInfo = routeInfo;
		}
	}
}