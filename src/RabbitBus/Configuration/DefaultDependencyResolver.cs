using System;

namespace RabbitBus.Configuration
{
	class DefaultDependencyResolver : IDependencyResolver
	{
		public object Resolve(Type handlerType)
		{
			return Activator.CreateInstance(handlerType);
		}
	}
}