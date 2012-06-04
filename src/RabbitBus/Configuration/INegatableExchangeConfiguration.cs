namespace RabbitBus.Configuration
{
	public interface INegatableExchangeConfiguration
	{
		IExchangeConfiguration Durable();
		IExchangeConfiguration AutoDelete();
	}
}