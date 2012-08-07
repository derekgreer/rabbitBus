using RabbitMQ.Client;

namespace RabbitBus
{
	public interface ISubscription
	{
		void Start();
		void Stop();
		void Renew(IConnection connection);
	}
}