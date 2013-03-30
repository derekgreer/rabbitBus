namespace RabbitBus.Configuration
{
	public class DeadLetterConfiguration : IDeadLetterConfiguration
	{
		public string ExchangeName { get; set; }
		public string RoutingKey { get; set; }

		public DeadLetterConfiguration(string deadLetterExchangeName, string routingKey)
		{
			ExchangeName = deadLetterExchangeName;
			RoutingKey = routingKey;
		}
	}
}