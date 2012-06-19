using System;

namespace RabbitBus.Configuration.Internal
{
	public class AutoSubscription
	{
		public AutoSubscription(Type messageType, object messageHandler)
		{
			MessageType = messageType;
			MessageHandler = messageHandler;
		}

		public Type MessageType { get; set; }
		public object MessageHandler { get; set; }
	}
}