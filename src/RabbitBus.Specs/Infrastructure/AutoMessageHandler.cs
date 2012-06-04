using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Infrastructure
{
	public class AutoMessageHandler : IMessageHandler<AutoMessage>
	{
		public static AutoMessage Message { get; set; }

		public void Handle(IMessageContext<AutoMessage> messageContext)
		{
			Message = messageContext.Message;
		}
	}
}