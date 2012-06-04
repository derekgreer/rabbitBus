using System;
using System.Collections.Generic;

namespace RabbitBus.Configuration.Internal
{
	class RouteConfiguration<TRouteInfo> : IRouteConfiguration<TRouteInfo> where TRouteInfo : class
	{
		readonly IDictionary<Type, IRouteInfoLookupStrategy<TRouteInfo>> _strategies =
			new Dictionary<Type, IRouteInfoLookupStrategy<TRouteInfo>>();

		public TRouteInfo GetRouteInfo(Type messageType)
		{
			return GetRouteInfoFromStrategy(messageType, _strategies.Values.ToQueue());
		}

		static TRouteInfo GetRouteInfoFromStrategy(Type messageType, Queue<IRouteInfoLookupStrategy<TRouteInfo>> values)
		{
			if (values.Count == 0)
				return null;

			TRouteInfo routeInfo = values.Dequeue().LookupRouteInfo(messageType);
			if (routeInfo != null)
			{
				return routeInfo;
			}

			return GetRouteInfoFromStrategy(messageType, values);
		}

		public void AddPolicy<TRouteInfoLookupStrategy>(Type messageType, TRouteInfo routeInfo)
		{
			IRouteInfoLookupStrategy<TRouteInfo> strategy = _strategies[typeof (TRouteInfoLookupStrategy)];
			strategy.Register(messageType, routeInfo);
		}

		public void AddStrategy<TMessage>() where TMessage : IRouteInfoLookupStrategy<TRouteInfo>, new()
		{
			_strategies.Add(typeof (TMessage), new TMessage());
		}
	}
}