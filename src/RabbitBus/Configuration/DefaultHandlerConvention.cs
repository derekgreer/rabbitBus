using System;

namespace RabbitBus.Configuration
{
	public class DefaultHandlerConvention : IHandlerConvention
	{
		public bool ShouldHandle(Type messageType, Type handlerType)
		{
			return handlerType.Name.Equals(messageType.Name + "Handler");
		}
	}
}