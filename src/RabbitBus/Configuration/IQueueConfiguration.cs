namespace RabbitBus.Configuration
{
	public interface IQueueConfiguration
	{
		INegatableQueueConfiguration Not { get; }
		IQueueConfiguration AutoDelete();
		IQueueConfiguration Durable();
		IQueueConfiguration AutoAcknowledge();
		IQueueConfiguration UnacknowledgeLimit(ushort count);
	}
}