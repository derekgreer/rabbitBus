using RabbitMQ.Client;

namespace RabbitBus.Configuration
{
	public interface IDeadLetterStrategy
	{
		void Publish(IBasicProperties basicProperties, byte[] body);
		void SetConnection(IConnection connection);
	}
}