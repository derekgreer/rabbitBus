using System;

namespace RabbitBus
{
	public class AutoSubscription
	{
		public AutoSubscription(Type messageType, Type messageHandlerType)
		{
			MessageType = messageType;
			MessageHandlerType = messageHandlerType;
		}

		public Type MessageType { get; set; }
		public Type MessageHandlerType { get; set; }
	}
}