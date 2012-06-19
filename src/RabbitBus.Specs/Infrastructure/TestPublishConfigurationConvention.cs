using System;
using RabbitBus.Configuration;

namespace RabbitBus.Specs.Infrastructure
{
	class TestPublishConfigurationConvention : PublishConfigurationConventionBase
	{
		readonly string _exchangeName;

		public TestPublishConfigurationConvention(string exchangeName)
		{
			_exchangeName = exchangeName;
		}

		public override bool ShouldRegister(Type type)
		{
			return type.Name.Equals("AutoMessage");
		}

		public override string GetExchangeName(Type type)
		{
			return _exchangeName;
		}
	}
}