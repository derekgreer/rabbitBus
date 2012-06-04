namespace RabbitBus.Configuration
{
	public interface INegatableQueueConfiguration
	{
		IQueueConfiguration Durable();
		IQueueConfiguration AutoAcknowledge();
		IQueueConfiguration AutoDelete();
	}
}