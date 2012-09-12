using System;
using System.Threading;
using RabbitBus.Logging;

namespace RabbitBus.Specs.Infrastructure
{
	class ConsoleLogger : ILogger
	{
		public void Write(LogEntry logEntry)
		{
			int id = Thread.CurrentThread.ManagedThreadId;
			Console.WriteLine(string.Format("Thread Id:{0} [{1}] - {2}", id, logEntry.Severity, logEntry.Message));
		}
	}
}