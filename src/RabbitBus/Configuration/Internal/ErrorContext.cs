using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitBus.Configuration.Internal
{
	class ErrorContext : IErrorContext
	{
		readonly IModel _channel;
		readonly BasicDeliverEventArgs _eventArgs;
		readonly IDeadLetterStrategy _deadLetterStrategy;

		public ErrorContext(IModel channel, BasicDeliverEventArgs eventArgs, IDeadLetterStrategy strategy)
		{
			_channel = channel;
			_eventArgs = eventArgs;
			_deadLetterStrategy = strategy;
		}

		public void RejectMessage(bool requeue)
		{
			if (_channel.IsOpen)
			{
				_deadLetterStrategy.Publish(_eventArgs.BasicProperties, _eventArgs.Body);
				_channel.BasicReject(_eventArgs.DeliveryTag, requeue);
			}
		}
	}
}