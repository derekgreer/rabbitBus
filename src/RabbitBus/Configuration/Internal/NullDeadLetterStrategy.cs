using RabbitMQ.Client;

namespace RabbitBus.Configuration.Internal
{
	class NullDeadLetterStrategy : IDeadLetterStrategy
	{
		public void Publish(IBasicProperties basicProperties, byte[] body)
		{
		}

		public void SetConnection(IConnection connection)
		{
		}
	}
}