namespace RabbitBus.Configuration
{
	public interface IQueueInfo
	{
		bool IsDurable { get; set; }
		bool IsAutoDelete { get; set; }
		bool IsAutoAcknowledge { get; set; }
		INegatableQueueConfiguration Not { get; }
	}
}