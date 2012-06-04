using System;
using System.Collections.Generic;

namespace RabbitBus.Configuration.Internal
{
	class MappingRouteInfoLookupStrategy<TRouteInfo> : IRouteInfoLookupStrategy<TRouteInfo> where TRouteInfo : class
	{
		readonly IDictionary<Type, TRouteInfo> _routeInfoDictionary =
			new Dictionary<Type, TRouteInfo>();

		public void Register(Type messageType, TRouteInfo routeInfo)
		{
			_routeInfoDictionary.Add(messageType, routeInfo);
		}

		public TRouteInfo LookupRouteInfo(Type messageType)
		{
			if (_routeInfoDictionary.ContainsKey(messageType))
				return _routeInfoDictionary[messageType];

			return null;
		}
	}
}