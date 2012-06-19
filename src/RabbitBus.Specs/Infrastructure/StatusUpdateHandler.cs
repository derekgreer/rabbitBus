using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Infrastructure
{
	public class StatusUpdateHandler : IMessageHandler<StatusUpdate>
	{
		public static StatusUpdate Message { get; set; }

		public void Handle(IMessageContext<StatusUpdate> messageContext)
		{
			Message = messageContext.Message;
		}
	}
}