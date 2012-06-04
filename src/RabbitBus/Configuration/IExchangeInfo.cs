namespace RabbitBus.Configuration
{
	public interface IExchangeInfo
	{
		bool IsDurable { get; set; }
		string ExchangeType { get; set; }
		bool IsAutoDelete { get; set; }
	}
}