using System.Diagnostics;

namespace RabbitBus.Logging
{
	public class LogEntry
	{
		public string Message { get; set; }

		public TraceEventType Severity { get; set; }

		public LogEntry()
		{
			// Defaults
			Severity = TraceEventType.Information;
		}
	}
}