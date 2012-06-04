namespace RabbitBus.Configuration
{
	public interface IErrorContext
	{
		void RejectMessage(bool requeue);
	}
}