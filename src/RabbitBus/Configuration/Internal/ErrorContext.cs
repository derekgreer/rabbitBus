using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus.Configuration.Internal
{
	class ErrorContext : IErrorContext
	{
		readonly IModel _channel;
		readonly BasicDeliverEventArgs _eventArgs;

		public ErrorContext(IModel channel, BasicDeliverEventArgs eventArgs /* IDeadLetterStrategy strategy */)
		{
			_channel = channel;
			_eventArgs = eventArgs;
		}

		public void RejectMessage(bool requeue)
		{
			if (_channel.IsOpen)
			{
				_channel.BasicReject(_eventArgs.DeliveryTag, requeue);
			}
		}
	}
}