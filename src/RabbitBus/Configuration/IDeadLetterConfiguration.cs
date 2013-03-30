namespace RabbitBus.Configuration
{
	public interface IDeadLetterConfiguration
	{
		string ExchangeName { get; set; }
		string RoutingKey { get; set; }
	}
}