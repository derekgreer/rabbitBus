namespace RabbitBus.Configuration
{

	public interface IExchangeTypeConfiguration
	{
		IExchangeConfiguration Fanout();
		IExchangeConfiguration Direct();
		IExchangeConfiguration Headers();
		IExchangeConfiguration Topic();
	}

	public interface IExchangeConfiguration : IExchangeTypeConfiguration
	{
		INegatableExchangeConfiguration Not { get; }
		IExchangeConfiguration AutoDelete();
		IExchangeConfiguration Durable();
	}
}