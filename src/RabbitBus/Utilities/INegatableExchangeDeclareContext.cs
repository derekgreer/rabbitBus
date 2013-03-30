namespace RabbitBus.Utilities
{
	public interface INegatableExchangeDeclareContext
	{
		IExchangeDeclareContext Durable();
		IExchangeDeclareContext AutoDelete();
	}
}