using System;
using Machine.Specifications;
using RabbitBus.Logging;
using RabbitBus.Specs.Infrastructure;
using RabbitBus.Specs.TestTypes;

namespace RabbitBus.Specs.Integration
{
	[Integration]
	[Subject("Logging")]
	public class when_configuring_a_logger
	{
		const string ExchangeName = "1E2FC706-EAE8-4252-BE22-53D9F19FB005";
		static bool _configuredLoggerUsed;
		static Bus _bus;

		Establish context = () =>
			{
				_bus = new BusBuilder().Configure(ctx => ctx.WithLogger(new TestLogger(() => _configuredLoggerUsed = true))
				                                         	.Publish<TestMessage>().WithExchange(ExchangeName)).Build();
				_bus.Connect();
			};

		Cleanup after = () => _bus.Close();

		Because of = () => _bus.Publish(new TestMessage("test"));

		It should_use_the_configured_logger = () => _configuredLoggerUsed.ShouldBeTrue();
	}

	class TestLogger : ILogger
	{
		readonly Func<bool> _loggingCallback;

		public TestLogger(Func<bool> loggingCallback)
		{
			_loggingCallback = loggingCallback;
		}

		public void Write(LogEntry logEntry)
		{
			_loggingCallback();
		}
	}
}