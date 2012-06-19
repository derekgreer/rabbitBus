using System;
using RabbitBus.Configuration;

namespace RabbitBus.Specs.Infrastructure
{
	public class NamedAutoMessageConsumeConfigurationConvention : ConsumeConfigurationConventionBase
	{
		readonly string _exchangeName;
		readonly string _queueName;

		public NamedAutoMessageConsumeConfigurationConvention(string exchangeName, string queueName)
		{
			_exchangeName = exchangeName;
			_queueName = queueName;
		}

		public override bool ShouldRegister(Type type)
		{
			return type.Name.Equals("AutoMessage");
		}

		public override string GetExchangeName(Type type)
		{
			return _exchangeName;
		}

		public override string GetQueueName(Type type)
		{
			return _queueName;
		}
	}
}