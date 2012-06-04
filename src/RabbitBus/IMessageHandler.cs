namespace RabbitBus
{
	public interface IMessageHandler<in TMessage> : IMessageHandler
	{
		void Handle(IMessageContext<TMessage> messageContext);
	}

	public interface IMessageHandler {}
}