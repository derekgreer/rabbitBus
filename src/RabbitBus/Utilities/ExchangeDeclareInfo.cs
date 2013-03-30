namespace RabbitBus.Utilities
{
	public interface IExchangeDeclareInfo
	{
		string Name { get; set; }
		string ExchangeType { get; set; }
		bool IsDurable { get; set; }
		bool IsAutoDelete { get; set; }
	}
}