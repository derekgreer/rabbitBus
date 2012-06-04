namespace RabbitBus.Configuration
{
	public interface ISerializationStrategy
	{
		string ContentType { get; }
		string ContentEncoding { get; }
		byte[] Serialize<T>(T message);
		T Deserialize<T>(byte[] bytes);
	}
}