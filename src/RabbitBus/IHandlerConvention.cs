using System;

namespace RabbitBus
{
	public interface IHandlerConvention
	{
		bool ShouldHandle(Type messageType, Type handlerType);
	}
}