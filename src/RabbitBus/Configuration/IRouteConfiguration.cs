using System;

namespace RabbitBus.Configuration
{
	public interface IRouteConfiguration<TRouteInfo> where TRouteInfo : class
	{
		TRouteInfo GetRouteInfo(Type messageType);
		void AddPolicy<TRouteInfoLookupStrategy>(Type messageType, TRouteInfo routeInfo);
		void AddStrategy<TMessage>() where TMessage : IRouteInfoLookupStrategy<TRouteInfo>, new();
	}
}