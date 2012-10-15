using System;

namespace RabbitBus.Configuration
{
	public interface IDependencyResolver
	{
		object Resolve(Type handlerType);
	}
}