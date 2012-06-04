using System;
using RabbitBus.Logging;

namespace RabbitBus.Specs.Infrastructure
{
	class ConsoleLogger : ILogger
	{
		public void Write(LogEntry logEntry)
		{
			Console.WriteLine(string.Format("{0}:{1}", logEntry.Severity, logEntry.Message));
		}
	}
}