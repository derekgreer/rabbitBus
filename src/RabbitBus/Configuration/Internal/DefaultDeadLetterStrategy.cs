using System;
using System.Diagnostics;
using RabbitBus.Logging;
using RabbitMQ.Client;

namespace RabbitBus.Configuration.Internal
{
	class DefaultDeadLetterStrategy : IDeadLetterStrategy, IDisposable
	{
		readonly string _queueName = "deadletter";
		IModel _channel;
		IConnection _connection;
		bool _disposed;

		public DefaultDeadLetterStrategy()
		{
		}

		public DefaultDeadLetterStrategy(string queueName)
		{
			_queueName = queueName;
		}

		public void Publish(IBasicProperties basicProperties, byte[] body)
		{
			_channel.QueueDeclare(_queueName, true, false, false, null);
			_channel.BasicPublish("", _queueName, basicProperties, body);
		}

		public void SetConnection(IConnection connection)
		{
			_connection = connection;
			_channel = _connection.CreateModel();
			_channel.ModelShutdown += ChannelModelShutdown;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		void ChannelModelShutdown(IModel model, ShutdownEventArgs reason)
		{
			Logger.Current.Write("Disposing of channel for DefaultDeadLetterStrategy", TraceEventType.Information);
			_channel.Dispose();
			_channel = null;
		}

		~DefaultDeadLetterStrategy()
		{
			Dispose(false);
		}

		public void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (_channel != null && _channel.IsOpen)
					{
						_channel.Close();
					}
				}
				_disposed = true;
			}
		}
	}
}