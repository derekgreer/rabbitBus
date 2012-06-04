using System.Diagnostics;

namespace RabbitBus.Logging
{
	public interface ILogger
	{
		void Write(LogEntry logEntry);
	}

	public static class LoggerExtensions
	{
		public static void Write(this ILogger logger, string message, TraceEventType severity)
		{
			var logEntry = new LogEntry
			               	{
			               		Message = message,
			               		Severity = severity
			               	};
			logger.Write(logEntry);
		}
	}
}