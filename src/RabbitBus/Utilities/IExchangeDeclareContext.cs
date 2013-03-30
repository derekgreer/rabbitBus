namespace RabbitBus.Utilities
{
	public interface IExchangeDeclareContext
	{
		IExchangeDeclareContext Durable();
		IExchangeDeclareContext AutoDelete();
		INegatableExchangeDeclareContext Not { get; }
	}
}