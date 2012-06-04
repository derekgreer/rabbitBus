namespace RabbitBus.Logging
{
	public static class Logger
	{
		static ILogger _logger;

		public static ILogger Current
		{
			get { return (_logger ?? (_logger = new NullLogger())); }
			set { _logger = value; }
		}

		public static void UseLogger(ILogger logger)
		{
			_logger = logger;
		}

		class NullLogger : ILogger
		{
			public void Write(LogEntry logEntry)
			{
			}
		}
	}
}