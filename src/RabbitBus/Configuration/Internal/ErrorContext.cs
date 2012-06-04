using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus.Configuration.Internal
{
	class ErrorContext : IErrorContext
	{
		readonly IModel _channel;
		readonly BasicDeliverEventArgs _eventArgs;

		public ErrorContext(IModel channel, BasicDeliverEventArgs eventArgs)
		{
			_channel = channel;
			_eventArgs = eventArgs;
		}

		public void RejectMessage(bool requeue)
		{
			if (_channel.IsOpen)
			{
				if (!requeue)
				{
					_channel.QueueDeclare("deadletter", true, false, false, null);
					_channel.BasicPublish("", "deadletter", _eventArgs.BasicProperties, _eventArgs.Body);
				}

				_channel.BasicReject(_eventArgs.DeliveryTag, requeue);
			}
		}
	}
}