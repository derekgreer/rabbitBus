using System;
using RabbitBus.Configuration;

namespace RabbitBus.Specs.Infrastructure
{
	public class TestSubscriptionConvention : SubscriptionConventionBase
	{
		public override bool ShouldRegister(Type type)
		{
			return type.Name.Contains("AutoMessage");
		}

		public override string GetExchangeName(Type type)
		{
			return "AutoMessage";
		}

		public override string GetQueueName(Type type)
		{
			return "AutoMessage";
		}
	}
}