namespace RabbitBus
{
	public interface IMessageHandler<in TMessage>
	{
		void Handle(IMessageContext<TMessage> messageContext);
	}
}