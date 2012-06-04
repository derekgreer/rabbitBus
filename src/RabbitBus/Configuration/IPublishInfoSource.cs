namespace RabbitBus.Configuration
{
	public interface IPublishInfoSource
	{
		IPublishInfo PublishInfo { get; }
	}
}