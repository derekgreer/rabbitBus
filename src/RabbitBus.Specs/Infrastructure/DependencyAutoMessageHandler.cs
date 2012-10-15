using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Infrastructure
{
	public class DependencyAutoMessageHandler : IMessageHandler<DependencyAutoMessage>
	{
		public DependencyAutoMessageHandler(INeededDependency neededDependency)
		{
		}

		public static AutoMessage Message { get; set; }

		public void Handle(IMessageContext<DependencyAutoMessage> messageContext)
		{
			Message = messageContext.Message;
		}
	}

	public interface INeededDependency
	{
	}

	class NeededDependency : INeededDependency
	{
	}
}