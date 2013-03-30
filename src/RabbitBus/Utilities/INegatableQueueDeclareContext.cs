namespace RabbitBus.Utilities
{
	public interface INegatableQueueDeclareContext
	{
		IQueueDeclareContext AutoDelete();
		IQueueDeclareContext Durable();
		IQueueDeclareContext Exclusive();
	}
}