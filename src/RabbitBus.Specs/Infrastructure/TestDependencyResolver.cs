using System;
using RabbitBus.Configuration;

namespace RabbitBus.Specs.Infrastructure
{
	class TestDependencyResolver : IDependencyResolver
	{
		public object Resolve(Type handlerType)
		{
			if (handlerType == typeof (DependencyAutoMessageHandler))
			{
				return new DependencyAutoMessageHandler(null);
			}

			return Activator.CreateInstance(handlerType);
		}
	}
}